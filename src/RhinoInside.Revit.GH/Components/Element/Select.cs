using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using ARDB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components.Elements
{
  using External.ApplicationServices.Extensions;

  public class ElementSelect : ZuiComponent
  {
    public override Guid ComponentGuid => new Guid("3E44D6BB-5F49-40E8-A2C4-53E5E3A63DDC");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    protected override string IconTag => "SDE";

    public ElementSelect() : base
    (
      name: "Select Element",
      nickname: "SelElems",
      description: "Adds or remove elements from active selection",
      category: "Revit",
      subCategory: "Element"
    )
    { }

    protected override ParamDefinition[] Inputs => inputs;
    static readonly ParamDefinition[] inputs =
    {
      new ParamDefinition
      (
        new Parameters.GraphicalElement()
        {
          Name = "Element",
          NickName = "E",
          Description = "Element to access selection state",
        }
      ),
      new ParamDefinition
      (
        new Param_Boolean()
        {
          Name = "Selected",
          NickName = "S",
          Description = "New state for Element Pin",
          Optional = true
        },
        ParamRelevance.Primary
      ),
    };

    protected override ParamDefinition[] Outputs => outputs;
    static readonly ParamDefinition[] outputs =
    {
      new ParamDefinition
      (
        new Parameters.GraphicalElement()
        {
          Name = "Element",
          NickName = "E",
          Description = "Element to access selection state",
        }
      ),
      new ParamDefinition
      (
        new Param_Boolean()
        {
          Name = "Selected",
          NickName = "S",
          Description = "State for Element Pin",
        },
        ParamRelevance.Primary
      ),
    };

    readonly Dictionary<ARDB.Document, HashSet<ARDB.ElementId>> Selection = new Dictionary<ARDB.Document, HashSet<ARDB.ElementId>>();
    protected override void BeforeSolveInstance()
    {
      base.BeforeSolveInstance();

      Revit.ActiveDBApplication.GetOpenDocuments(out var projects, out var families);

      foreach (var doc in projects.Concat(families))
      {
        var uiDoc = new Autodesk.Revit.UI.UIDocument(doc);
        Selection.Add(uiDoc.Document, new HashSet<ARDB.ElementId>(uiDoc.Selection.GetElementIds()));
      }
    }

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      if (!Params.GetData(DA, "Element", out Types.Element element, x => x.IsValid)) return;
      else DA.SetData("Element", element);

      if (Params.GetData(DA, "Selected", out bool? selected))
      {
        if (Selection.TryGetValue(element.Document, out var selection))
        {
          if (selected.Value) selection.Add(element.Id);
          else selection.Remove(element.Id);
        }
      }
      else
      {
        Params.TrySetData(DA, "Selected", () =>
        {
          if (Selection.TryGetValue(element.Document, out var selection))
            selected = selection.Contains(element.Id);

          return selected;
        });
      }
    }

    protected override void AfterSolveInstance()
    {
      try
      {
        var input = Params.Input<IGH_Param>("Selected");
        if (input is object)
        {
          var output = Params.Output<IGH_Param>("Selected");
          if (output is object)
            output.VolatileData.ClearData();

          // Make Selection effective
          foreach (var selection in Selection)
          {
            using (var uiDocument = new Autodesk.Revit.UI.UIDocument(selection.Key))
            {
              uiDocument.Selection.SetElementIds(selection.Value);
            }
          }

          // Update Selected output
          if (output is object && Params.Output<IGH_Param>("Element") is IGH_Param element)
          {
            output.AddVolatileDataTree
            (
              element.VolatileData,
              (Types.Element x) => x is object && Selection.TryGetValue(x?.Document, out var sel) ? new GH_Boolean(sel.Contains(x.Id)) : null
            );
          }
        }
      }
      finally
      {
        Selection.Clear();
      }

      base.AfterSolveInstance();
    }
  }
}
