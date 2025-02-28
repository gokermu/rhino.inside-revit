using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Grasshopper.Kernel;
using Rhino.Geometry;
using ARDB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components
{
  using Convert.Geometry;
  using Convert.System.Collections.Generic;
  using External.DB.Extensions;
  using Kernel.Attributes;

  [ComponentVersion(introduced: "1.3"), ComponentRevitAPIVersion(min: "2022.0")]
  public class CeilingByOutline : ReconstructElementComponent
  {
    public override Guid ComponentGuid => new Guid("A39BBDF2-78F2-4501-BB6E-F9CC3E83516E");
    public override GH_Exposure Exposure => SDKCompliancy(GH_Exposure.primary);

    public CeilingByOutline() : base
    (
      name: "Add Ceiling",
      nickname: "Ceiling",
      description: "Given its outline curve, it adds a Ceiling element to the active Revit document",
      category: "Revit",
      subCategory: "Architecture"
    )
    { }

    bool Reuse(ref ARDB.Ceiling element, IList<Curve> boundaries, ARDB.CeilingType type, ARDB.Level level)
    {
      if (element is null) return false;

      if (!(element.GetSketch() is ARDB.Sketch sketch && Types.Sketch.SetProfile(sketch, boundaries, Vector3d.ZAxis)))
        return false;

      if (element.GetTypeId() != type.Id)
      {
        if (ARDB.Element.IsValidType(element.Document, new ARDB.ElementId[] { element.Id }, type.Id))
        {
          if (element.ChangeTypeId(type.Id) is ARDB.ElementId id && id != ARDB.ElementId.InvalidElementId)
            element = element.Document.GetElement(id) as ARDB.Ceiling;
        }
        else return false;
      }

      bool succeed = true;
      succeed &= element.get_Parameter(ARDB.BuiltInParameter.LEVEL_PARAM).Update(level.Id);

      return succeed;
    }

    void ReconstructCeilingByOutline
    (
      [Optional, NickName("DOC")]
      ARDB.Document document,

      [Description("New Ceiling")]
      ref ARDB.Ceiling ceiling,

      IList<Curve> boundary,
      Optional<ARDB.CeilingType> type,
      Optional<ARDB.Level> level
    )
    {
#if REVIT_2022
      if (boundary is null) return;

      var tol = GeometryTolerance.Model;
      var normal = default(Vector3d);
      var maxArea = 0.0; var maxIndex = 0;
      for (int index = 0; index < boundary.Count; ++index)
      {
        var loop = boundary[index];
        if (loop is null) return;
        var plane = default(Plane);
        if
        (
          loop.IsShort(tol.ShortCurveTolerance) ||
          !loop.IsClosed ||
          !loop.TryGetPlane(out plane, tol.VertexTolerance) ||
          plane.ZAxis.IsParallelTo(Vector3d.ZAxis, tol.AngleTolerance) == 0
        )
          ThrowArgumentException(nameof(boundary), "Boundary loop curves should be a set of valid horizontal, coplanar and closed curves.");

        boundary[index] = loop.Simplify(CurveSimplifyOptions.All, tol.VertexTolerance, tol.AngleTolerance) ?? loop;

        using (var properties = AreaMassProperties.Compute(loop, tol.VertexTolerance))
        {
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

        index++;
      }

      if (type.HasValue && type.Value.Document.IsEquivalent(document) == false)
        ThrowArgumentException(nameof(type));

      if (level.HasValue && level.Value.Document.IsEquivalent(document) == false)
        ThrowArgumentException(nameof(level));

      SolveOptionalType(document, ref type, ARDB.ElementTypeGroup.CeilingType, nameof(type));

      SolveOptionalLevel(document, boundary, ref level, out var bbox);

      if (boundary.Count == 0)
      {
        ceiling = default;
      }
      else if (!Reuse(ref ceiling, boundary, type.Value, level.Value))
      {
        var parametersMask = new ARDB.BuiltInParameter[]
        {
          ARDB.BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
          ARDB.BuiltInParameter.ELEM_FAMILY_PARAM,
          ARDB.BuiltInParameter.ELEM_TYPE_PARAM,
          ARDB.BuiltInParameter.LEVEL_PARAM,
          ARDB.BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM
        };


        var curveLoops = boundary.ConvertAll(GeometryEncoder.ToCurveLoop);

        ReplaceElement(ref ceiling, ARDB.Ceiling.Create(document, curveLoops, type.Value.Id, level.Value.Id, default, 0.0), parametersMask);
      }

      if (ceiling is object)
      {
        var heightAboveLevel = bbox.Min.Z / Revit.ModelUnits - level.Value.GetElevation();
        ceiling.get_Parameter(ARDB.BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM)?.Update(heightAboveLevel);
      }
#endif
    }
  }
}
