using TMPro;
using UnityEngine;

namespace Wirelet
{
    internal class UIUtils
    {
        internal static TextMeshPro CreateText(GameObject objectToAddTo, FontStyles fontStyle, float outlineWidth, Color outlineColor, Color textColor, float fontSize, Vector4 margin, string text)
		{
			TextMeshPro textMeshPro = objectToAddTo.GetComponent<TextMeshPro>() ?? objectToAddTo.AddComponent<TextMeshPro>();
			textMeshPro.fontStyle = fontStyle;
			textMeshPro.outlineWidth = outlineWidth;
			textMeshPro.outlineColor = outlineColor;
			textMeshPro.color = textColor;
			textMeshPro.fontSize = fontSize;
			textMeshPro.alignment = TextAlignmentOptions.Center;
			textMeshPro.margin = margin;
			textMeshPro.text = text;
			textMeshPro.richText = true;
			return textMeshPro;
		}
	}
}