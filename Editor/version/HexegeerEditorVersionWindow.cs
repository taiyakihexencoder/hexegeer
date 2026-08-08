using System.Collections.Generic;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class HexegeerEditorVersionWindow : EditorWindow {
		private const int DESCRIPTION_LENGTH = 400;

		private int currentOpenIndex = -1;

		private void OnEnable() {
			titleContent = new GUIContent("Version");
			ScrollPane pane = new ScrollPane()
				.Padding(16f);

			rootVisualElement.Add(pane);
			CreateView(pane);
		}

		private void CreateView(ScrollPane pane) {
			pane.Clear();

			VersionSettings settings = VersionSettings.instance;

			List<VersionSettings.Version> versions = settings.Versions ?? new List<VersionSettings.Version>();
			int major = -1;
			int minor = -1;
			internallib.Foldout parent = null;
			for (int i = 0; i < versions.Count; ++i) {
				int index = i;
				if (major != versions[i].major) {
					if (parent != null) { pane.Add(parent); }

					parent = new internallib.Foldout(Text.H3($"version {versions[i].major}.{versions[i].minor}"), currentOpenIndex == index)
						.Margin(horizontal: 16f);
					parent.onExpandedStateChanged += expand => { currentOpenIndex = expand ? index : -1; };
					parent.Add(VersionButtons(i, () => CreateView(pane)));
					major = versions[i].major;
					minor = versions[i].minor;
				} else if (minor != versions[i].minor) {
					if (parent != null) { pane.Add(parent); }

					parent = new internallib.Foldout(Text.H3($"version {versions[i].major}.{versions[i].minor}"), currentOpenIndex == index)
						.Margin(horizontal: 16f);
					parent.onExpandedStateChanged += expand => { currentOpenIndex = expand ? index : -1; };
					parent.Add(VersionButtons(i, () => CreateView(pane)));

					minor = versions[i].minor;
				}

				Text text = Text.Body($" - {versions[i].major}.{versions[i].minor}.{versions[i].patch}")
					.Margin(left:32);

				Text lengthText = Text.Body("")
					.TextAlign(TextAnchor.MiddleRight);
				lengthText.text = $"({versions[i].description.Length}/{DESCRIPTION_LENGTH})";

				TextField description = new TextField();
				description.SetValueWithoutNotify(versions[i].description);
				description.maxLength = DESCRIPTION_LENGTH;
				description.style.paddingLeft = 32;
				description.style.paddingRight = 32;
				description.style.width = new Length(100, LengthUnit.Percent);
				description.multiline = true;
				description.RegisterValueChangedCallback(v => {
					versions[index].description = v.newValue;
					lengthText.text = $"({v.newValue.Length}/{DESCRIPTION_LENGTH})";
					settings.UpdateVersion(index, versions[index]);
				});

				parent.AddChildren(text, description, lengthText);
			}

			if (parent != null) { pane.Add(parent); }

			ClickButton addMajorButton = ClickButton.Create()
				.Label("+ Major Version");
			addMajorButton.OnClicked += () => {
				settings.AddMajorVersion(settings.Versions.Count > 0 ? settings.Versions[settings.Versions.Count-1] : null);
				CreateView(pane);
			};
			pane.Add(addMajorButton);
		}

		private VisualElement VersionButtons(int index, System.Action replace) {
			VersionSettings settings = VersionSettings.instance;

			Row row = new Row()
				.HorzontalArrangement(Justify.FlexEnd);

			if (NextMinorVersion(index) == 0) {
				ClickButton addMinor = ClickButton.Create()
					.Label("+ Minor Version");
				addMinor.OnClicked += () => {
					settings.AddMinorVersion(settings.Versions[index]);
					replace();
				};
				row.AddChildren(addMinor, new Spacer(width: 20));
			}

			ClickButton plus = ClickButton.Create()
				.Label("+");
			plus.OnClicked += () => {
				if (index == settings.Versions.Count-1) {
					settings.AddPatchVersion(settings.Versions[settings.Versions.Count-1]);
					replace();
				} else {
					VersionSettings.Version version = settings.Versions[index];
					for (int i = index+1; i < settings.Versions.Count; ++i) {
						VersionSettings.Version v = settings.Versions[i];
						if (v.major != version.major || v.minor != version.minor) {
							settings.AddPatchVersion(settings.Versions[i-1]);
							replace();
							return;
						}
					}
					settings.AddPatchVersion(settings.Versions[settings.Versions.Count-1]);
					replace();
				}
			};

			ClickButton minus = ClickButton.Create()
				.Label("-");
			minus.OnClicked += () => {
				if (index == settings.Versions.Count-1) {
					settings.RemoveVersion(index);
					replace();
				} else {
					VersionSettings.Version v = settings.Versions[index];
					for (int i = index+1; i < settings.Versions.Count; ++i) {
						if (settings.Versions[i].major != v.major || settings.Versions[i].minor != v.minor) {
							settings.RemoveVersion(i-1);
							replace();
							return;
						}
					}
					settings.RemoveVersion(settings.Versions.Count-1);
					replace();
				}
			};

			row.AddChildren(plus, new Spacer(width:20), minus);
			return row;
		}

		private int NextMinorVersion(int index) {
			VersionSettings settings = VersionSettings.instance;
			int major = settings.Versions[index].major;
			int minor = settings.Versions[index].minor;
			for (int i = index+1; i < settings.Versions.Count; ++i) {
				if (settings.Versions[i].major != major || settings.Versions[i].minor != minor) {
					return settings.Versions[i].minor;
				}
			}
			return 0;
		}
	}
}