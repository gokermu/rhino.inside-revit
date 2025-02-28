using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using ARDB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components.Structure
{
  using Convert.Geometry;
  using Convert.System.Collections.Generic;
  using External.DB.Extensions;
  using GH.Exceptions;

  [ComponentVersion(introduced: "1.14")]
  public class AddFoundationSlab : ElementTrackerComponent
  {
    public override Guid ComponentGuid => new Guid("83E76F44-2738-46F0-A2A2-AA609C5F6E98");
    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    public AddFoundationSlab() : base
    (
      name: "Add Foundation (Slab)",
      nickname: "S-Foundation",
      description: "Given its outline curve, it adds a foundation slab element to the active Revit document",
      category: "Revit",
      subCategory: "Structure"
    )
    { }

    protected override ParamDefinition[] Inputs => inputs;
    static readonly ParamDefinition[] inputs =
    {
      new ParamDefinition
      (
        new Parameters.Document()
        {
          Name = "Document",
          NickName = "DOC",
          Description = "Document",
          Optional = true
        }, ParamRelevance.Occasional
      ),
      new ParamDefinition
       (
        new Param_Curve
        {
          Name = "Boundary",
          NickName = "B",
          Description = "Slab boundary profile",
          Access = GH_ParamAccess.list
        }
      ),
      new ParamDefinition
       (
        new Parameters.HostObjectType
        {
          Name = "Type",
          NickName = "T",
          Description = "Slab type",
          Optional = true,
          SelectedBuiltInCategory = ARDB.BuiltInCategory.OST_StructuralFoundation
        }, ParamRelevance.Primary
      ),
      new ParamDefinition
       (
        new Parameters.Level
        {
          Name = "Level",
          NickName = "L",
          Description = "Slab base level",
          Optional = true
        }, ParamRelevance.Primary
      ),
      new ParamDefinition
       (
        new Param_Boolean
        {
          Name = "Structural",
          NickName = "S",
          Description = "Whether slab is structural or not",
        }.SetDefaultVale(true), ParamRelevance.Secondary
      )
    };

    protected override ParamDefinition[] Outputs => outputs;
    static readonly ParamDefinition[] outputs =
    {
      new ParamDefinition
      (
        new Parameters.Floor()
        {
          Name = _Slab_,
          NickName = _Slab_.Substring(0, 1),
          Description = $"Output {_Slab_}",
        }
      )
    };

    const string _Slab_ = "Slab";
    static readonly ARDB.BuiltInParameter[] ExcludeUniqueProperties =
    {
      ARDB.BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
      ARDB.BuiltInParameter.ELEM_FAMILY_PARAM,
      ARDB.BuiltInParameter.ELEM_TYPE_PARAM,
      ARDB.BuiltInParameter.LEVEL_PARAM,
      ARDB.BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM,
      ARDB.BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL
    };

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      if (!Parameters.Document.TryGetDocumentOrCurrent(this, DA, "Document", out var doc) || !doc.IsValid) return;

      ReconstructElement<ARDB.Floor>
      (
        doc.Value, _Slab_, slab =>
        {
          // Input
          if (!Params.GetDataList(DA, "Boundary", out IList<Curve> boundary)) return null;

          var tol = GeometryTolerance.Model;
          var normal = default(Vector3d);
          var maxArea = 0.0; var maxIndex = 0;
          for (int index = 0; index < boundary.Count; ++index)
          {
            var loop = boundary[index];
            if (loop is null) return null;
            var plane = default(Plane);
            if
            (
              loop.IsShort(tol.ShortCurveTolerance) ||
              !loop.IsClosed ||
              !loop.TryGetPlane(out plane, tol.VertexTolerance) ||
              plane.ZAxis.IsParallelTo(Vector3d.ZAxis, tol.AngleTolerance) == 0
            )
              throw new RuntimeArgumentException(nameof(boundary), "Boundary loop curves should be a set of valid horizontal, coplanar and closed curves.", boundary);

            boundary[index] = loop.Simplify(CurveSimplifyOptions.All, tol.VertexTolerance, tol.AngleTolerance) ?? loop;

            using (var properties = AreaMassProperties.Compute(loop, tol.VertexTolerance))
            {
              if (properties is null) return null;
              if (properties.Area > maxArea)
              {
                normal = plane.Normal;
                maxArea = properties.Area;
                maxIndex = index;

                var orientation = loop.ClosedCurveOrientation(Plane.WorldXY);
                if (orientation == CurveOrientation.CounterClockwise)
                  normal.Reverse();
              }
            }
          }

#if !REVIT_2022
          if (boundary.Count > 1)
          {
            boundary = new Curve[] { boundary[maxIndex] };
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Multiple boundary curves are only supported on Revit 2022 or above.");
          }
#endif
          var bbox = Rhino.Geometry.BoundingBox.Empty;
          foreach (var geometry in boundary)
            bbox.Union(geometry.GetBoundingBox(true));

          if (!Parameters.ElementType.GetDataOrDefault(this, DA, "Type", out Types.ElementType type, doc, ARDB.ElementTypeGroup.FootingSlabType)) return null;

          if (!(type.Value is ARDB.FloorType floorType))
            throw new RuntimeArgumentException(nameof(type), $"Type '{type.Nomen}' is not a valid foundation slab type.");
          else if (!floorType.IsFoundationSlab)
            throw new RuntimeArgumentException(nameof(type), $"Type '{type.Nomen}' is not a valid foundation slab type.{Environment.NewLine}Consider use 'Add Floor' component.");

          if (!Parameters.Level.GetDataOrDefault(this, DA, "Level", out Types.Level level, doc, bbox.IsValid ? bbox.Min.Z : double.NaN)) return null;
          if (!Params.TryGetData(DA, "Structural", out bool? structural)) return null;

          // Compute
          slab = Reconstruct(slab, doc.Value, boundary, floorType, level.Value, structural ?? true);

          if (slab is object)
          {
            var heightAboveLevel = bbox.Min.Z / Revit.ModelUnits - level.Value.GetElevation();
            slab.get_Parameter(ARDB.BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM)?.Update(heightAboveLevel);
          }

          DA.SetData(_Slab_, slab);
          return slab;
        }
      );
    }

    bool Reuse(ref ARDB.Floor floor, IList<Curve> boundaries, ARDB.FloorType type, ARDB.Level level, bool structural)
    {
      if (floor is null) return false;

      if (!(floor.GetSketch() is ARDB.Sketch sketch && Types.Sketch.SetProfile(sketch, boundaries, Vector3d.ZAxis)))
        return false;

      if (floor.GetTypeId() != type.Id)
      {
        if (ARDB.Element.IsValidType(floor.Document, new ARDB.ElementId[] { floor.Id }, type.Id))
        {
          if (floor.ChangeTypeId(type.Id) is ARDB.ElementId id && id != ARDB.ElementId.InvalidElementId)
            floor = floor.Document.GetElement(id) as ARDB.Floor;
        }
        else return false;
      }

      bool succeed = true;
      succeed &= floor.get_Parameter(ARDB.BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL).Update(structural ? 1 : 0);
      succeed &= floor.get_Parameter(ARDB.BuiltInParameter.LEVEL_PARAM).Update(level.Id);

      return succeed;
    }

    ARDB.Floor Create(ARDB.Document document, IList<Curve> boundary, ARDB.FloorType type, ARDB.Level level, bool structural)
    {
#if REVIT_2022
      var curveLoops = boundary.ConvertAll(GeometryEncoder.ToCurveLoop);
      var floor = ARDB.Floor.Create(document, curveLoops, type.Id, level.Id, structural, default, 0.0);
#else
      var curveArray = boundary[0].ToBoundedCurveArray();
      var floor = document.Create.NewFoundationSlab(curveArray, type, level, structural, ARDB.XYZ.BasisZ);
#endif

      // We turn off analytical model off by default
      floor.get_Parameter(ARDB.BuiltInParameter.STRUCTURAL_ANALYTICAL_MODEL)?.Update(false);
      floor.get_Parameter(ARDB.BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL)?.Update(structural);
      return floor;
    }

    ARDB.Floor Reconstruct(ARDB.Floor floor, ARDB.Document doc, IList<Curve> boundary, ARDB.FloorType type, ARDB.Level level, bool structural)
    {
      if (!Reuse(ref floor, boundary, type, level, structural))
      {
        floor = floor.ReplaceElement
        (
          Create(doc, boundary, type, level, structural),
          ExcludeUniqueProperties
        );
      }

      return floor;
    }
  }
}
