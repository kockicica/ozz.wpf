using System;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace ozz.wpf.Controls;

public class CustomAutoCompleteBox : AutoCompleteBox, IStyleable {

    #region IStyleable Members

    public Type StyleKey { get; } = typeof(AutoCompleteBox);

    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        var textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        if (textBox != null) {
            textBox.Classes.Add("clearButton");
        }

    }
}