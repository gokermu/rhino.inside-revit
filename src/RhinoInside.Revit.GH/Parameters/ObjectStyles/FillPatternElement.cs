using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using ARDB = Autodesk.Revit.DB;

namespace RhinoInside.Revit.GH.Parameters
{
  public class FillPatternElement : Element<Types.FillPatternElement, ARDB.FillPatternElement>
  {
    public override GH_Exposure Exposure => GH_Exposure.quarternary;
    public override Guid ComponentGuid => new Guid("EFDDB3D7-CF2A-4972-B1C4-29374BB89149");

    public FillPatternElement() : base
    (
      name: "Fill Pattern",
      nickname: "Fill Pattern",
      description: "Contains a collection of Revit fill pattern elements",
      category: "Params",
      subcategory: "Revit Elements"
    )
    { }

    #region UI
    public override void Menu_AppendActions(ToolStripDropDown menu)
    {
      base.Menu_AppendActions(menu);

      var activeApp = Revit.ActiveUIApplication;
      var commandId = Autodesk.Revit.UI.RevitCommandId.LookupPostableCommandId(Autodesk.Revit.UI.PostableCommand.FillPatterns);
      Menu_AppendItem
      (
        menu, $"Open Fill Patterns…",
        (sender, arg) => External.UI.EditScope.PostCommand(activeApp, commandId),
        activeApp.ActiveUIDocument is object && activeApp.CanPostCommand(commandId), false
      );
    }

    protected override void Menu_AppendPromptOne(ToolStripDropDown menu)
    {
      if (SourceCount != 0) return;
      if (Revit.ActiveUIDocument?.Document is null) return;

      var listBox = new ListBox
      {
        Sorted = true,
        BorderStyle = BorderStyle.FixedSingle,
        Width = (int) (200 * GH_GraphicsUtil.UiScale),
        Height = (int) (100 * GH_GraphicsUtil.UiScale),
        DisplayMember = nameof(Types.Element.DisplayName)
      };
      listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;

      var patternTargetBox = new ComboBox
      {
        Sorted = true,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = (int) (200 * GH_GraphicsUtil.UiScale),
        Tag = listBox
      };
      patternTargetBox.SelectedIndexChanged += PatternTargetBox_SelectedIndexChanged;
      patternTargetBox.SetCueBanner("Fill Pattern target filter…");

      using (var collector = new ARDB.FilteredElementCollector(Revit.ActiveUIDocument.Document))
      {
        var targets = collector.
                        OfClass(typeof(ARDB.FillPatternElement)).
                        Cast<ARDB.FillPatternElement>().
                        Select(x => x.GetFillPattern().Target).
                        Distinct();

        foreach (var target in targets)
          patternTargetBox.Items.Add(target);

        if (PersistentValue?.Value is ARDB.FillPatternElement current)
        {
          var targetIndex = 0;
          foreach (var patternTarget in patternTargetBox.Items.Cast<ARDB.FillPatternTarget>())
          {
            if (current.GetFillPattern().Target == patternTarget)
            {
              patternTargetBox.SelectedIndex = targetIndex;
              break;
            }
            targetIndex++;
          }
        }
        else patternTargetBox.SelectedIndex = (int) ARDB.FillPatternTarget.Drafting;
      }

      Menu_AppendCustomItem(menu, patternTargetBox);
      Menu_AppendCustomItem(menu, listBox);
    }

    private void PatternTargetBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (sender is ComboBox comboBox)
      {
        if (comboBox.Tag is ListBox listBox)
          RefreshPatternList(listBox, comboBox.SelectedItem as ARDB.FillPatternTarget?);
      }
    }

    private void RefreshPatternList(ListBox listBox, ARDB.FillPatternTarget? patternTarget)
    {
      var doc = Revit.ActiveUIDocument.Document;

      listBox.SelectedIndexChanged -= ListBox_SelectedIndexChanged;
      listBox.BeginUpdate();
      listBox.Items.Clear();
      listBox.Items.Add(new Types.FillPatternElement());

      using (var collector = new ARDB.FilteredElementCollector(doc).OfClass(typeof(ARDB.FillPatternElement)))
      {
        var patterns = collector.
                       Cast<ARDB.FillPatternElement>().
                       Where(x => !patternTarget.HasValue || x.GetFillPattern().Target == patternTarget);

        foreach (var pattern in patterns)
          listBox.Items.Add(new Types.FillPatternElement(pattern));
      }

      listBox.SelectedIndex = listBox.Items.Cast<Types.FillPatternElement>().IndexOf(PersistentValue, 0).FirstOr(-1);
      listBox.EndUpdate();
      listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
    }

    private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (sender is ListBox listBox)
      {
        if (listBox.SelectedIndex != -1)
        {
          if (listBox.Items[listBox.SelectedIndex] is Types.FillPatternElement value)
          {
            RecordPersistentDataEvent($"Set: {value}");
            PersistentData.Clear();
            PersistentData.Append(value);
            OnObjectChanged(GH_ObjectEventType.PersistentData);
          }
        }

        ExpireSolution(true);
      }
    }
    #endregion
  }
}
