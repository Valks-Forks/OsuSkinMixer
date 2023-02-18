using Godot;
using System;
using OsuSkinMixer.Models.SkinOptions;

namespace OsuSkinMixer.Components;

public partial class SkinOptionComponent : HBoxContainer
{
	public TextureButton ArrowButton { get; private set; }

	public TextureButton ResetButton { get; private set; }

	public Button Button { get; private set; }

	private Label Label;

    public override void _Ready()
    {
		ArrowButton = GetNode<TextureButton>("ArrowButton");
		Label = GetNode<Label>("Label");
		Button = GetNode<Button>("Button");
		ResetButton = GetNode<TextureButton>("Button/ResetButton");
    }

	public void SetValues(SkinOption option, int indentLayer)
	{
		TooltipText = option.ToString();
		Name = option.Name;
		Label.Text = option.Name;

		if (option is not ParentSkinOption)
		{
			// Option has no children, so hide button to expand option.
			// Don't set visible to false so the button still takes up space. 
			ArrowButton.Disabled = true;
			ArrowButton.Modulate = new Color(0, 0, 0, 0);
		}

		var indent = new Panel()
		{
			CustomMinimumSize = new Vector2(indentLayer * 30, 1),
			Modulate = new Color(0, 0, 0, 0),
		};

		AddChild(indent);
		MoveChild(indent, 0);
	}
}
