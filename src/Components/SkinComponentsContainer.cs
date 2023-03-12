using Godot;
using OsuSkinMixer.Models;
using OsuSkinMixer.Statics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsuSkinMixer.Components;

public partial class SkinComponentsContainer : VBoxContainer
{
	public Action<OsuSkin> SkinSelected { get; set; }

	public SkinComponent BestMatch => GetChildren().Cast<SkinComponent>().FirstOrDefault(c => c.Visible);

	private readonly List<SkinComponent> _disabledSkinComponents = new();

	private bool _skinComponentsInitialised;

	public PackedScene SkinComponentScene
	{
		get => _skinComponentScene;
		set
		{
			_skinComponentScene = value;

			if (_skinComponentsInitialised)
				InitialiseSkinComponents();
		}
	}

    private PackedScene _skinComponentScene;

	public override void _Ready()
	{
		OsuData.SkinAdded += OnSkinAdded;
		OsuData.SkinModified += OnSkinModified;
		OsuData.SkinRemoved += OnSkinRemoved;

		TreeExiting += () =>
		{
			OsuData.SkinAdded -= OnSkinAdded;
			OsuData.SkinModified -= OnSkinModified;
			OsuData.SkinRemoved -= OnSkinRemoved;
		};
	}

	public void InitialiseSkinComponents()
	{
		_skinComponentsInitialised = true;

		foreach (var child in GetChildren())
			child.QueueFree();

		foreach (OsuSkin skin in OsuData.Skins)
			AddChild(CreateSkinComponentFrom(skin));
	}

	public void FilterSkins(string filter)
	{
		foreach (var component in GetChildren().Cast<SkinComponent>())
        {
            component.Visible = component.Name.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase)
				&& !_disabledSkinComponents.Contains(component);
        }
	}

	public void DisableSkinComponent(OsuSkin skin)
	{
		var skinComponent = GetExistingComponentFromSkin(skin);
		skinComponent.Visible = false;
		_disabledSkinComponents.Add(skinComponent);
	}

	public void EnableSkinComponent(OsuSkin skin)
	{
		var skinComponent = GetExistingComponentFromSkin(skin);
		skinComponent.Visible = true;
		_disabledSkinComponents.Remove(skinComponent);
	}

	private SkinComponent CreateSkinComponentFrom(OsuSkin skin)
	{
		SkinComponent instance = SkinComponentScene.Instantiate<SkinComponent>();
		instance.Skin = skin;
		instance.Name = skin.Name;
		instance.Pressed += () => SkinSelected(skin);

		return instance;
	}

	private SkinComponent GetExistingComponentFromSkin(OsuSkin skin)
	{
		return GetChildren().Cast<SkinComponent>().FirstOrDefault(c => c.Skin.Name == skin.Name);
	}

	private void OnSkinAdded(OsuSkin skin)
	{
		var skinComponent = CreateSkinComponentFrom(skin);
		AddChild(skinComponent);
		MoveChild(skinComponent, Array.IndexOf(OsuData.Skins, skin));
	}

	private void OnSkinModified(OsuSkin skin)
	{
		var skinComponent = GetExistingComponentFromSkin(skin);
		skinComponent.Skin = skin;
		skinComponent.SetValues();
	}

	private void OnSkinRemoved(OsuSkin skin)
	{
		GetExistingComponentFromSkin(skin).QueueFree();
	}
}
