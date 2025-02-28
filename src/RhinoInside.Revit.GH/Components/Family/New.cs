using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using ARDB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Components.Families
{
  using Convert.Geometry;
  using External.DB.Extensions;

  public class FamilyNew : TransactionalComponent
  {
    public override Guid ComponentGuid => new Guid("82523911-309F-4A66-A4B9-CF21E0AC250E");
    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    protected override string IconTag => "N";

    public FamilyNew() : base
    (
      name: "New Component Family",
      nickname: "New",
      description: "Creates a new Family from a template.",
      category: "Revit",
      subCategory: "Component"
    )
    { }

    protected override ParamDefinition[] Inputs => inputs;
    static readonly ParamDefinition[] inputs =
    {
      new ParamDefinition
      (
        new Parameters.Document(),
        ParamRelevance.Occasional
      ),
      new ParamDefinition
      (
        new Param_FilePath()
        {
          Name = "Template",
          NickName = "T",
          Optional = true,
          FileFilter = "Family Template Files (*.rft)|*.rft"
        }, ParamRelevance.Primary
      ),
      new ParamDefinition
      (
        new Param_Boolean()
        {
          Name = "Overwrite",
          NickName = "O",
          Description = "Overwrite Family",
        }.
        SetDefaultVale(false),
        ParamRelevance.Primary
      ),
      new ParamDefinition
      (
        new Param_Boolean()
        {
          Name = "Overwrite Parameters",
          NickName = "OP",
          Description = "Overwrite Parameters",
        }.
        SetDefaultVale(false),
        ParamRelevance.Occasional
      ),
      new ParamDefinition
      (
        new Param_String()
        {
          Name = "Name",
          NickName = "N",
          Description = "Family Name",
        }
      ),
      new ParamDefinition
      (
        new Parameters.Category()
        {
          Name = "Category",
          NickName = "C",
          Description = "Family Category",
          Optional = true
        }, ParamRelevance.Primary
      ),
      new ParamDefinition
      (
        new Param_Geometry()
        {
          Name = "Geometry",
          NickName = "G",
          Description = "Family Geometry",
          Access = GH_ParamAccess.list,
          Optional = true
        }
      ),
    };

    protected override ParamDefinition[] Outputs => outputs;
    static readonly ParamDefinition[] outputs =
    {
      new ParamDefinition
      (
        new Parameters.Family()
        {
          Name = "Family",
          NickName = "F",
        }
      )
    };

    public override void AddedToDocument(GH_Document document)
    {
      if (Params.Input<IGH_Param>("Override Family") is IGH_Param overrideFamily)
        overrideFamily.Name = "Overwrite";

      if (Params.Input<IGH_Param>("Override Parameters") is IGH_Param overrideParameters)
        overrideParameters.Name = "Overwrite Parameters";

      base.AddedToDocument(document);
    }

    public static Dictionary<string, ARDB.ElementId> GetMaterialIdsByName(ARDB.Document doc)
    {
      var collector = new ARDB.FilteredElementCollector(doc);
      return collector.OfClass(typeof(ARDB.Material)).OfType<ARDB.Material>().
        GroupBy(x => x.Name).
        ToDictionary(x => x.Key, x => x.First().Id);
    }

    static GeometryBase AsGeometryBase(IGH_GeometricGoo obj)
    {
      var scriptVariable = obj?.ScriptVariable();
      switch (scriptVariable)
      {
        case Point3d point:     return new Point(point);
        case Line line:         return new LineCurve(line);
        case Rectangle3d rect:  return rect.ToNurbsCurve();
        case Arc arc:           return new ArcCurve(arc);
        case Circle circle:     return new ArcCurve(circle);
        case Ellipse ellipse:   return ellipse.ToNurbsCurve();
        case Box box:           return box.ToBrep();
      }

      return (scriptVariable as Rhino.Geometry.GeometryBase)?.DuplicateShallow();
    }

    class PlaneComparer : IComparer<KeyValuePair<double[], ARDB.SketchPlane>>
    {
      public static PlaneComparer Instance = new PlaneComparer();

      int IComparer<KeyValuePair<double[], ARDB.SketchPlane>>.Compare(KeyValuePair<double[], ARDB.SketchPlane> x, KeyValuePair<double[], ARDB.SketchPlane> y)
      {
        var abcdX = x.Key;
        var abcdY = y.Key;

        const double tol = Rhino.RhinoMath.ZeroTolerance;

        var d = abcdX[3] - abcdY[3];
        if (d < -tol) return -1;
        if (d > +tol) return +1;

        var c = abcdX[2] - abcdY[2];
        if (c < -tol) return -1;
        if (c > +tol) return +1;

        var b = abcdX[1] - abcdY[1];
        if (b < -tol) return -1;
        if (b > +tol) return +1;

        var a = abcdX[0] - abcdY[0];
        if (a < -tol) return -1;
        if (a > +tol) return +1;

        return 0;
      }
    }

    ARDB.Category MapCategory(ARDB.Document project, ARDB.Document family, ARDB.ElementId categoryId, bool createIfNotExist = false)
    {
      if (categoryId.TryGetBuiltInCategory(out var _))
        return family.GetCategory(categoryId);

      try
      {
        if (ARDB.Category.GetCategory(project, categoryId) is ARDB.Category category)
        {
          if (family.OwnerFamily.FamilyCategory.SubCategories.Contains(category.Name) && family.OwnerFamily.FamilyCategory.SubCategories.get_Item(category.Name) is ARDB.Category subCategory)
            return subCategory;

          if (createIfNotExist)
            return family.Settings.Categories.NewSubcategory(family.OwnerFamily.FamilyCategory, category.Name);
        }
      }
      catch (Autodesk.Revit.Exceptions.InvalidOperationException) { }

      return null;
    }

    ARDB.GraphicsStyle MapGraphicsStyle(ARDB.Document project, ARDB.Document family, ARDB.ElementId graphicsStyleId, bool createIfNotExist = false)
    {
      try
      {
        if (project.GetElement(graphicsStyleId) is ARDB.GraphicsStyle graphicsStyle)
        {
          if (family.OwnerFamily.FamilyCategory.SubCategories.Contains(graphicsStyle.GraphicsStyleCategory.Name) && family.OwnerFamily.FamilyCategory.SubCategories.get_Item(graphicsStyle.GraphicsStyleCategory.Name) is ARDB.Category subCategory)
            return subCategory.GetGraphicsStyle(graphicsStyle.GraphicsStyleType);

          if (createIfNotExist)
            return family.Settings.Categories.NewSubcategory(family.OwnerFamily.FamilyCategory, graphicsStyle.GraphicsStyleCategory.Name).
                   GetGraphicsStyle(graphicsStyle.GraphicsStyleType);
        }
      }
      catch (Autodesk.Revit.Exceptions.InvalidOperationException) { }

      return null;
    }

    static ARDB.ElementId MapMaterial(ARDB.Document project, ARDB.Document family, ARDB.ElementId materialId, bool createIfNotExist = false)
    {
      if (project.GetElement(materialId) is ARDB.Material material)
      {
        using (var collector = new ARDB.FilteredElementCollector(family).OfClass(typeof(ARDB.Material)))
        {
          if (collector.Cast<ARDB.Material>().FirstOrDefault(x => x.Name == material.Name) is ARDB.Material familyMaterial)
            return familyMaterial.Id;
        }

        if (createIfNotExist)
          return ARDB.Material.Create(family, material.Name);
      }

      return ARDB.ElementId.InvalidElementId;
    }

    class DeleteElementEnumerator<T> : IEnumerator<T> where T : ARDB.Element
    {
      readonly IEnumerator<T> enumerator;
      public DeleteElementEnumerator(IEnumerable<T> e) { enumerator = e.GetEnumerator(); }
      readonly List<ARDB.Element> elementsToDelete = new List<ARDB.Element>();

      public void Dispose()
      {
        while (MoveNext()) ;

        foreach (var element in elementsToDelete)
          element.Document.Delete(element.Id);

        enumerator.Dispose();
        DeleteCurrent = false;
      }

      public bool DeleteCurrent;
      public T Current => DeleteCurrent ? enumerator.Current : null;
      object IEnumerator.Current => Current;
      void IEnumerator.Reset() { enumerator.Reset(); DeleteCurrent = false; }
      public bool MoveNext()
      {
        if (DeleteCurrent)
          elementsToDelete.Add(Current);

        return DeleteCurrent = enumerator.MoveNext();
      }
    }

    bool Add
    (
      ARDB.Document doc,
      ARDB.Document familyDoc,
      Brep brep,
      DeleteElementEnumerator<ARDB.GenericForm> forms
    )
    {
      forms.MoveNext();
      if (brep.ToSolid() is ARDB.Solid solid)
      {
        if (forms.Current is ARDB.FreeFormElement freeForm)
        {
          freeForm.UpdateSolidGeometry(solid);
          forms.DeleteCurrent = false;
        }
        else freeForm = ARDB.FreeFormElement.Create(familyDoc, solid);

        brep.TryGetUserString(ARDB.BuiltInParameter.ELEMENT_IS_CUTTING.ToString(), out bool cutting, false);
        freeForm.get_Parameter(ARDB.BuiltInParameter.ELEMENT_IS_CUTTING).Update(cutting ? 1 : 0);

        if (!cutting)
        {
          ARDB.Category familySubCategory = null;
          if
          (
            brep.TryGetUserString(ARDB.BuiltInParameter.FAMILY_ELEM_SUBCATEGORY.ToString(), out ARDB.ElementId subCategoryId) &&
            ARDB.Category.GetCategory(doc, subCategoryId) is ARDB.Category subCategory
          )
          {
            if (subCategory.Parent.Id == familyDoc.OwnerFamily.FamilyCategory.Id)
            {
              familySubCategory = MapCategory(doc, familyDoc, subCategoryId, true);
            }
            else
            {
              if (subCategory.Parent is null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"'{subCategory.Name}' is not subcategory of '{familyDoc.OwnerFamily.FamilyCategory.Name}'");
              else
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"'{subCategory.Parent.Name} : {subCategory.Name}' is not subcategory of '{familyDoc.OwnerFamily.FamilyCategory.Name}'");
            }
          }

          if (familySubCategory is null)
            freeForm.get_Parameter(ARDB.BuiltInParameter.FAMILY_ELEM_SUBCATEGORY).Update(ARDB.ElementId.InvalidElementId);
          else
            freeForm.Subcategory = familySubCategory;

          brep.TryGetUserString(ARDB.BuiltInParameter.IS_VISIBLE_PARAM.ToString(), out var visible, true);
          freeForm.get_Parameter(ARDB.BuiltInParameter.IS_VISIBLE_PARAM).Update(visible ? 1 : 0);

          brep.TryGetUserString(ARDB.BuiltInParameter.GEOM_VISIBILITY_PARAM.ToString(), out var visibility, 57406);
          freeForm.get_Parameter(ARDB.BuiltInParameter.GEOM_VISIBILITY_PARAM).Update(visibility);

          brep.TryGetUserString(ARDB.BuiltInParameter.MATERIAL_ID_PARAM.ToString(), out ARDB.ElementId materialId);
          var familyMaterialId = MapMaterial(doc, familyDoc, materialId, true);
          freeForm.get_Parameter(ARDB.BuiltInParameter.MATERIAL_ID_PARAM).Update(familyMaterialId);
        }

        return cutting;
      }

      return false;
    }

    void Add
    (
      ARDB.Document doc,
      ARDB.Document familyDoc,
      Curve curve,
      List<KeyValuePair<double[], ARDB.SketchPlane>> planesSet,
      DeleteElementEnumerator<ARDB.CurveElement> curves
    )
    {
      if (curve.TryGetPlane(out var plane))
      {
        var familyGraphicsStyle = default(ARDB.GraphicsStyle);
        {
          ARDB.Category familySubCategory = null;
          if
          (
            curve.TryGetUserString(ARDB.BuiltInParameter.FAMILY_ELEM_SUBCATEGORY.ToString(), out ARDB.ElementId subCategoryId) &&
            doc.GetCategory(subCategoryId) is ARDB.Category subCategory
          )
          {
            if (subCategoryId.ToBuiltInCategory() == ARDB.BuiltInCategory.OST_InvisibleLines)
            {
              familySubCategory = MapCategory(doc, familyDoc, subCategoryId, true);
            }
            else if (subCategoryId == familyDoc.OwnerFamily.FamilyCategory.Id)
            {
              familySubCategory = MapCategory(doc, familyDoc, subCategoryId, true);
            }
            else if (subCategory?.Parent?.Id == familyDoc.OwnerFamily.FamilyCategory.Id)
            {
              familySubCategory = MapCategory(doc, familyDoc, subCategoryId, true);
            }
            else
            {
              if (subCategory.Parent is null)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"'{subCategory.Name}' is not subcategory of '{familyDoc.OwnerFamily.FamilyCategory.Name}'");
              else
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"'{subCategory.Parent.Name} : {subCategory.Name}' is not subcategory of '{familyDoc.OwnerFamily.FamilyCategory.Name}'");
            }
          }

          curve.TryGetUserString(ARDB.BuiltInParameter.FAMILY_CURVE_GSTYLE_PLUS_INVISIBLE.ToString(), out var graphicsStyleType, ARDB.GraphicsStyleType.Projection);

          familyGraphicsStyle = familySubCategory?.GetGraphicsStyle(graphicsStyleType);
        }

        curve.TryGetUserString(ARDB.BuiltInParameter.MODEL_OR_SYMBOLIC.ToString(), out bool symbolic, true);
        curve.TryGetUserString(ARDB.BuiltInParameter.IS_VISIBLE_PARAM.ToString(), out var visible, true);
        curve.TryGetUserString(ARDB.BuiltInParameter.GEOM_VISIBILITY_PARAM.ToString(), out var visibility, 57406);

        if (familyDoc.OwnerFamily.FamilyCategory.Id.ToBuiltInCategory() == ARDB.BuiltInCategory.OST_DetailComponents)
        {
          using (var collector = new ARDB.FilteredElementCollector(familyDoc).OfClass(typeof(ARDB.View)))
          {
            var view = collector.FirstElement() as ARDB.View;
            foreach (var c in curve.ToCurveMany())
            {
              curves.MoveNext();

              if (curves.Current is ARDB.DetailCurve detailCurve && detailCurve.GeometryCurve.IsSameKindAs(c))
              {
                detailCurve.SetGeometryCurve(c, true);
                curves.DeleteCurrent = false;
              }
              else detailCurve = familyDoc.FamilyCreate.NewDetailCurve(view, c);

              detailCurve.get_Parameter(ARDB.BuiltInParameter.IS_VISIBLE_PARAM).Update(visible ? 1 : 0);
              detailCurve.get_Parameter(ARDB.BuiltInParameter.GEOM_VISIBILITY_PARAM).Update(visibility);

              if (familyGraphicsStyle is object)
                detailCurve.LineStyle = familyGraphicsStyle;

              if (!symbolic)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Curves on a Detail will not display in 3D views");
            }
          }
        }
        else
        {
          var abcd = plane.GetPlaneEquation();
          int index = planesSet.BinarySearch(new KeyValuePair<double[], ARDB.SketchPlane>(abcd, null), PlaneComparer.Instance);
          if (index < 0)
          {
            var entry = new KeyValuePair<double[], ARDB.SketchPlane>(abcd, ARDB.SketchPlane.Create(familyDoc, plane.ToPlane()));
            index = ~index;
            planesSet.Insert(index, entry);
          }
          var sketchPlane = planesSet[index].Value;

          foreach (var c in curve.ToCurveMany())
          {
            curves.MoveNext();

            if (symbolic)
            {
              if (curves.Current is ARDB.SymbolicCurve symbolicCurve && symbolicCurve.GeometryCurve.IsSameKindAs(c))
              {
                symbolicCurve.SetSketchPlaneAndCurve(sketchPlane, c);
                curves.DeleteCurrent = false;
              }
              else symbolicCurve = familyDoc.FamilyCreate.NewSymbolicCurve(c, sketchPlane);

              symbolicCurve.get_Parameter(ARDB.BuiltInParameter.IS_VISIBLE_PARAM).Update(visible ? 1 : 0);
              symbolicCurve.get_Parameter(ARDB.BuiltInParameter.GEOM_VISIBILITY_PARAM).Update(visibility);

              if (familyGraphicsStyle is object)
                symbolicCurve.Subcategory = familyGraphicsStyle;
            }
            else
            {
              if (curves.Current is ARDB.ModelCurve modelCurve && modelCurve.GeometryCurve.IsSameKindAs(c))
              {
                modelCurve.SetSketchPlaneAndCurve(sketchPlane, c);
                curves.DeleteCurrent = false;
              }
              else modelCurve = familyDoc.FamilyCreate.NewModelCurve(c, sketchPlane);

              modelCurve.get_Parameter(ARDB.BuiltInParameter.IS_VISIBLE_PARAM).Update(visible ? 1 : 0);
              modelCurve.get_Parameter(ARDB.BuiltInParameter.GEOM_VISIBILITY_PARAM).Update(visibility);

              if (familyGraphicsStyle is object)
                modelCurve.Subcategory = familyGraphicsStyle;
            }
          }
        }
      }
    }

    void Add
    (
      ARDB.Document doc,
      ARDB.Document familyDoc,
      IEnumerable<Curve> loops,
      ARDB.HostObject host,
      DeleteElementEnumerator<ARDB.Opening> openings
    )
    {
      var profile = loops.SelectMany(x => x.ToCurveMany()).ToCurveArray();
      var opening = familyDoc.FamilyCreate.NewOpening(host, profile);
    }

    static bool TryGetDefaultLanguageFolderName(ARDB.Document doc, out string folderName)
    {
      switch (doc.Application.Language)
      {
        case Autodesk.Revit.ApplicationServices.LanguageType.English_USA:
#if REVIT_2018
        case Autodesk.Revit.ApplicationServices.LanguageType.English_GB:
#endif
        switch (doc.DisplayUnitSystem)
          {
            case ARDB.DisplayUnit.METRIC:   folderName = "English";           return true;
            case ARDB.DisplayUnit.IMPERIAL: folderName = "English-Imperial";  return true;
          }
          break;
        case Autodesk.Revit.ApplicationServices.LanguageType.German:              folderName = "German"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Spanish:             folderName = "Spanish"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.French:              folderName = "French"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Italian:             folderName = "Italian"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Dutch:               folderName = "Dutch"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Chinese_Simplified:  folderName = "Chinese"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Chinese_Traditional: folderName = "Traditional Chinese"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Japanese:            folderName = "Japanese"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Korean:              folderName = "Korean"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Russian:             folderName = "Russian"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Czech:               folderName = "Czech"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Polish:              folderName = "Polish"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Hungarian:           folderName = "Hungarian"; return true;
        case Autodesk.Revit.ApplicationServices.LanguageType.Brazilian_Portuguese:folderName = "Portuguese"; return true;
      }

      folderName = default;
      return false;
    }

    static string GetDefaultTemplateFileName(ARDB.Document doc, ARDB.ElementId categoryId)
    {
      if (categoryId?.ToBuiltInCategory() == ARDB.BuiltInCategory.OST_Mass)
      {
        switch (doc.Application.Language)
        {
          case Autodesk.Revit.ApplicationServices.LanguageType.English_USA:
#if REVIT_2018
          case Autodesk.Revit.ApplicationServices.LanguageType.English_GB:
#endif
            switch (doc.DisplayUnitSystem)
            {
              case ARDB.DisplayUnit.METRIC:   return @"Conceptual Mass\Metric Mass";
              case ARDB.DisplayUnit.IMPERIAL: return @"Conceptual Mass\Mass";
            }
            break;
          case Autodesk.Revit.ApplicationServices.LanguageType.German: return @"Entwurfskörper\M_Körper";
          case Autodesk.Revit.ApplicationServices.LanguageType.Spanish: return @"Masas conceptuales\Masa métrica";
          case Autodesk.Revit.ApplicationServices.LanguageType.French: return @"Volume conceptuel\Volume métrique";
          case Autodesk.Revit.ApplicationServices.LanguageType.Italian: return @"Massa concettuale\Massa metrica";
          case Autodesk.Revit.ApplicationServices.LanguageType.Chinese_Simplified: return @"概念体量\公制体量";
          case Autodesk.Revit.ApplicationServices.LanguageType.Chinese_Traditional: return @"概念量體\公制量體";
          case Autodesk.Revit.ApplicationServices.LanguageType.Japanese: return @"コンセプト マス\マス(メートル単位)";
          case Autodesk.Revit.ApplicationServices.LanguageType.Korean: return @"개념 질량\미터법 질량";
          case Autodesk.Revit.ApplicationServices.LanguageType.Russian: return @"Концептуальный формообразующий элемент\Метрическая система, формообразующий элемент";
          case Autodesk.Revit.ApplicationServices.LanguageType.Czech: return null;
          case Autodesk.Revit.ApplicationServices.LanguageType.Polish: return @"Bryła koncepcyjna\Bryła (metryczna)";
          case Autodesk.Revit.ApplicationServices.LanguageType.Hungarian: return null;
          case Autodesk.Revit.ApplicationServices.LanguageType.Brazilian_Portuguese: return @"Massa conceitual\Massa métrica";
        }

        return null;
      }

      switch (doc.Application.Language)
      {
        case Autodesk.Revit.ApplicationServices.LanguageType.English_USA:
#if REVIT_2018
        case Autodesk.Revit.ApplicationServices.LanguageType.English_GB:
#endif
          switch (doc.DisplayUnitSystem)
          {
            case ARDB.DisplayUnit.METRIC:   return @"Metric Generic Model";
            case ARDB.DisplayUnit.IMPERIAL: return @"Generic Model";
          }
          break;
        case Autodesk.Revit.ApplicationServices.LanguageType.German: return @"Allgemeines Modell";
        case Autodesk.Revit.ApplicationServices.LanguageType.Spanish: return @"Modelo genérico métrico";
        case Autodesk.Revit.ApplicationServices.LanguageType.French: return @"Modèle générique métrique";
        case Autodesk.Revit.ApplicationServices.LanguageType.Italian: return @"Modello generico metrico";
        case Autodesk.Revit.ApplicationServices.LanguageType.Chinese_Simplified: return @"公制常规模型";
        case Autodesk.Revit.ApplicationServices.LanguageType.Chinese_Traditional: return @"公制常规模型";
        case Autodesk.Revit.ApplicationServices.LanguageType.Japanese: return @"一般モデル(メートル単位)";
        case Autodesk.Revit.ApplicationServices.LanguageType.Korean: return @"미터법 일반 모델";
        case Autodesk.Revit.ApplicationServices.LanguageType.Russian: return @"Метрическая система, типовая модель";
        case Autodesk.Revit.ApplicationServices.LanguageType.Czech: return @"Obecný model";
        case Autodesk.Revit.ApplicationServices.LanguageType.Polish: return @"Model ogólny (metryczny)";
        case Autodesk.Revit.ApplicationServices.LanguageType.Hungarian: return null;
        case Autodesk.Revit.ApplicationServices.LanguageType.Brazilian_Portuguese: return @"Modelo genérico métrico";
      }

      return null;
    }

    static string GetDefaultTemplatePath(ARDB.Document doc, ARDB.ElementId categoryId)
    {
      if (GetDefaultTemplateFileName(doc, categoryId) is string fileName)
      {
        if (TryGetDefaultLanguageFolderName(doc, out var language))
        {
          return Path.Combine
          (
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Autodesk",
            $"RVT {doc.Application.VersionNumber}",
            "Family Templates",
            language,
            fileName + ".rft"
          );
        }
      }

      return null;
    }

    static bool FindTemplatePath(ARDB.Document doc, ref string templatePath, out bool pathWasRelative)
    {
      pathWasRelative = !Path.IsPathRooted(templatePath);

      // Validate input
      foreach (var invalid in Path.GetInvalidPathChars())
      {
        if (templatePath.Contains(invalid))
          return false;
      }

      var components = templatePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      foreach (var component in components.Skip(pathWasRelative ? 0 : 1))
      {
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
          if (component.Contains(invalid))
            return false;
        }
      }

      if (!pathWasRelative)
        return File.Exists(templatePath);

      var folders = new List<string>();
      {
        // 1. Look next to the project
        if (File.Exists(doc.PathName))
          folders.Add(Path.GetDirectoryName(doc.PathName));

        // 2. Look into `Application.FamilyTemplatePath`
        if (Directory.Exists(doc.Application.FamilyTemplatePath))
          folders.Add(doc.Application.FamilyTemplatePath);

        // 3. Look into default family templates path
        {
          var defaultFamilyTemplatePath = Path.Combine
          (
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Autodesk",
            $"RVT {doc.Application.VersionNumber}",
            "Family Templates"
          );

          if (!folders.Contains(defaultFamilyTemplatePath) && Directory.Exists(defaultFamilyTemplatePath))
            folders.Add(defaultFamilyTemplatePath);
        }
      }

      // Look into each folder for the first match
      foreach (var folder in folders)
      {
        var template = components.Length == 1 ?
          Directory.EnumerateFiles(folder, components[0], SearchOption.TopDirectoryOnly).FirstOrDefault() :
          Path.GetFullPath(Path.Combine(folder, templatePath));

        if (File.Exists(template))
        {
          templatePath = template;
          return true;
        }
      }

      return false;
    }

    protected override void TrySolveInstance(IGH_DataAccess DA)
    {
      if (!Parameters.Document.GetDataOrDefault(this, DA, "Document", out var doc)) return;
      if (!Params.TryGetData(DA, "Overwrite", out bool? overwrite)) return;
      if (!overwrite.HasValue) overwrite = false;
      if (!Params.TryGetData(DA, "Overwrite Parameters", out bool? overwriteParameters)) return;
      if (!overwriteParameters.HasValue) overwriteParameters = overwrite;
      if (!Params.GetData(DA, "Name", out string name)) return;
      if (!Params.TryGetData(DA, "Category", out ARDB.ElementId categoryId)) return;

      var geometry = new List<IGH_GeometricGoo>();
      var updateGeometry = !(!DA.GetDataList("Geometry", geometry) && Params.Input[Params.IndexOfInputParam("Geometry")].SourceCount == 0);

      var templatePath = string.Empty;
      if (!doc.TryGetFamily(name, out var family, categoryId))
      {
        var useTemplate = categoryId?.ToBuiltInCategory() == ARDB.BuiltInCategory.OST_Mass;
        if (!useTemplate)
        {
          if (doc.IsFamilyDocument && doc.OwnerFamily.FamilyPlacementType == ARDB.FamilyPlacementType.ViewBased)
            useTemplate = true;
        }
        
        if (!Params.TryGetData(DA, "Template", out templatePath)) return;
        if (templatePath is object || useTemplate)
        {
          templatePath = templatePath ?? GetDefaultTemplatePath(doc, categoryId);

          if (!Path.HasExtension(templatePath))
            templatePath += ".rft";

          if (FindTemplatePath(doc, ref templatePath, out var pathWasRelative))
          {
            if (pathWasRelative)
              AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Using template file from '{templatePath}'");
          }
          else
          {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Failed to found Template file '{templatePath}'.");
            return;
          }
        }
        else
        {
          using (var transaction = NewTransaction(doc))
          {
            transaction.Start(Name);

            family = doc.CreateWorkPlaneBasedSymbol(name).Family;
            overwrite = true;
            updateGeometry = true;

            CommitTransaction(doc, transaction);
          }
        }
      }

      var updateCategory = categoryId is object && family?.FamilyCategory.Id != categoryId;
      var updateName = family is null;
      if (family is null || (overwrite == true && (updateCategory || updateGeometry)))
      {
        try
        {
          if((family is null ? doc.Application.NewFamilyDocument(templatePath) : doc.EditFamily(family)) is var familyDoc)
          {
            try
            {
              using (var transaction = NewTransaction(familyDoc))
              {
                transaction.Start(Name);

                if (updateCategory && familyDoc.OwnerFamily.FamilyCategoryId != categoryId)
                {
                  try { familyDoc.OwnerFamily.FamilyCategoryId = categoryId; }
                  catch (Autodesk.Revit.Exceptions.ArgumentException e)
                  {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                    return;
                  }
                }

                if (updateGeometry)
                {
                  using (var forms = new DeleteElementEnumerator<ARDB.GenericForm>(new ARDB.FilteredElementCollector(familyDoc).OfClass(typeof(ARDB.GenericForm)).Cast<ARDB.GenericForm>().ToArray()))
                  using (var curves = new DeleteElementEnumerator<ARDB.CurveElement>(new ARDB.FilteredElementCollector(familyDoc).OfClass(typeof(ARDB.CurveElement)).Cast<ARDB.CurveElement>().Where(x => x.Category.Id.ToBuiltInCategory() != ARDB.BuiltInCategory.OST_SketchLines).ToArray()))
                  using (var openings = new DeleteElementEnumerator<ARDB.Opening>(new ARDB.FilteredElementCollector(familyDoc).OfClass(typeof(ARDB.Opening)).Cast<ARDB.Opening>().ToArray()))
                  using (var ctx = GeometryEncoder.Context.Push(familyDoc))
                  {
                    ctx.RuntimeMessage = (severity, message, invalidGeometry) =>
                      AddGeometryConversionError((GH_RuntimeMessageLevel) severity, message, invalidGeometry);

                    bool hasVoids = false;
                    var planesSet = new List<KeyValuePair<double[], ARDB.SketchPlane>>();
                    var planesSetComparer = new PlaneComparer();
                    var loops = new List<Rhino.Geometry.Curve>();

                    foreach (var geo in geometry.Select(x => AsGeometryBase(x)))
                    {
                      try
                      {
                        if (geo is Rhino.Geometry.Curve loop && geo.TryGetUserString("IS_OPENING_PARAM", out bool opening, false) && opening)
                        {
                          loops.Add(loop);
                        }
                        else
                        {
                          switch (geo)
                          {
                            case Rhino.Geometry.Brep brep: hasVoids |= Add(doc, familyDoc, brep, forms); break;
                            case Rhino.Geometry.Curve curve: Add(doc, familyDoc, curve, planesSet, curves); break;
                            default:
                              if (geo is object)
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"{geo.GetType().Name} is not supported and will be ignored");
                              break;
                          }
                        }
                      }
                      catch (Autodesk.Revit.Exceptions.InvalidOperationException e)
                      {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                      }
                    }

                    if (loops.Count > 0)
                    {
                      using (var hosts = new ARDB.FilteredElementCollector(familyDoc).OfClass(typeof(ARDB.HostObject)))
                      {
                        if (hosts.FirstOrDefault(x => x is ARDB.Wall || x is ARDB.Ceiling) is ARDB.HostObject host)
                          Add(doc, familyDoc, loops, host, openings);
                        else
                          AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No suitable host object is been found");
                      }
                    }

                    familyDoc.OwnerFamily.get_Parameter(ARDB.BuiltInParameter.FAMILY_ALLOW_CUT_WITH_VOIDS).Update(hasVoids ? 1 : 0);
                  }
                }

                CommitTransaction(familyDoc, transaction);
              }

              family = familyDoc.LoadFamily(doc, new FamilyLoadOptions(overwrite is true, overwriteParameters is true));
            }
            finally
            {
              familyDoc.Release();
            }

            if (updateName)
            {
              using (var transaction = NewTransaction(doc))
              {
                transaction.Start(Name);
                try { family.Name = name; }
                catch (Autodesk.Revit.Exceptions.ArgumentException e) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, e.Message); }

                if (doc.GetElement(family.GetFamilySymbolIds().First()) is ARDB.FamilySymbol symbol)
                  symbol.Name = name;

                CommitTransaction(doc, transaction);
              }
            }
          }
        }
        catch (Autodesk.Revit.Exceptions.ArgumentException e)
        {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
        }
      }
      else if (overwrite != true)
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Family '{name}' already loaded!");
      }

      DA.SetData("Family", family);
    }
  }
}
