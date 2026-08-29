using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using hexegeer.internallib;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace hexegeer.editor {
	public sealed class EditorGridView : VisualElement {
		private const float ROW_HEADER_WIDTH = 60f;
		private const float COLUMN_HEADER_HEIGHT = 20f;
		private const float CONTROL_PANEL_HEIGHT = 32f;

		private const float DEFAULT_CONTENT_HEIGHT = 25f;

		private const float BORDER_WIDTH = 1f;

		public class Column {
			public readonly int Id;
			public readonly string Name;
			public readonly ColumnType Type;

			public Column(int id, string name, ColumnType type) {
				Id = id;
				Name = name;
				Type = type;
			}
		}

		public enum ColumnType {
			INT,
			BOOL,
			FLOAT,
			STRING,
		}

		private readonly Column[] _columns;
		private List<float> _widths;
		private List<float> _heights;

		private ScrollView _background;
		private Box _foreground;
		private VisualElement _editField;
		private Box _rowNumberArea;
		private Box _columnNumberArea;
		private Row _controlPanel;
		private Box _gridArea;

		private List<VisualElement> _horizontalLines = new List<VisualElement>();
		private List<VisualElement> _foregroundViews = new List<VisualElement>();

		private int[] _intIds;
		private int[] _boolIds;
		private int[] _floatIds;
		private int[] _textIds;

		List<int>[] _intData = null;
		List<bool>[] _boolData = null;
		List<float>[] _floatData = null;
		List<string>[] _textData = null;

		public EditorGridView(string filePath, params Column[] columns) {
			style.width = new Length(100, LengthUnit.Percent);
			style.height = new Length(100, LengthUnit.Percent);

			_columns = columns;

			_widths = new List<float>();
			for (int i = 0; i < _columns.Length; ++i) {
				_widths.Add(100f);
			}

			_heights = new List<float>();

			List<int> intColumns = new List<int>();
			List<int> boolColumns = new List<int>();
			List<int> floatColumns = new List<int>();
			List<int> textColumns = new List<int>();
			for (int c = 0; c < columns.Length; ++c) {
				switch(columns[c].Type) {
					case ColumnType.INT: {
						intColumns.Add(c);
						break;
					}
					case ColumnType.BOOL: {
						boolColumns.Add(c);
						break;
					}
					case ColumnType.FLOAT: {
						floatColumns.Add(c);
						break;
					}
					case ColumnType.STRING: {
						textColumns.Add(c);
						break;
					}
				}
			}

			_intIds = new int[intColumns.Count];
			_intData = new List<int>[intColumns.Count];
			for (int i = 0; i < intColumns.Count; ++i) {
				_intIds[i] = columns[intColumns[i]].Id;
				_intData[i] = new List<int>();
			}

			_boolIds = new int[boolColumns.Count];
			_boolData = new List<bool>[boolColumns.Count];
			for (int i = 0; i < boolColumns.Count; ++i) {
				_boolIds[i] = columns[boolColumns[i]].Id;
				_boolData[i] = new List<bool>();
			}

			_floatIds = new int[floatColumns.Count];
			_floatData = new List<float>[floatColumns.Count];
			for (int i = 0; i < floatColumns.Count; ++i) {
				_floatIds[i] = columns[floatColumns[i]].Id;
				_floatData[i] = new List<float>();
			}

			_textIds = new int[textColumns.Count];
			_textData = new List<string>[textColumns.Count];
			for (int i = 0; i < textColumns.Count; ++i) {
				_textIds[i] = columns[textColumns[i]].Id;
				_textData[i] = new List<string>();
			}

			Load(filePath);

			_background = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
			_background.StretchToParentSize();
			Add(_background);

			DrawBackground();
		}

		private void Load(string filePath) {
			if (File.Exists(filePath)) {
				try {
					using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
						using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.UTF8)) {
							short columns = reader.ReadInt16();
							int rows = reader.ReadInt32();

							int[] ids = new int[columns];
							ColumnType[] types = new ColumnType[columns];

							short[][] textLength = new short[_textData.Length][];
							for (int i = 0; i < textLength.Length; ++i) {
								textLength[i] = new short[0];
							}

							for (int c = 0; c < columns; ++c) {
								switch(types[c]) {
									case ColumnType.INT: {
										for (int i = 0; i < _intIds.Length; ++i) {
											if (_intIds[i] == ids[c]) {
												_intData[i] = new List<int>(ReadIntArray(reader, rows));
												break;
											}
										}
										break;
									}
									case ColumnType.BOOL: {
										for (int i = 0; i < _boolIds.Length; ++i) {
											if (_boolIds[i] == ids[c]) {
												_boolData[i] = new List<bool>(ReadBoolArray(reader, rows));
												break;
											}
										}
										break;
									}
									case ColumnType.FLOAT: {
										for (int i = 0; i < _floatIds.Length; ++i) {
											if (_floatIds[i] == ids[c]) {
												_floatData[i] = new List<float>(ReadFloatArray(reader, rows));
												break;
											}
										}
										break;
									}
									case ColumnType.STRING: {
										for (int i = 0; i < _textIds.Length; ++i) {
											if (_textIds[i] == ids[c]) {
												textLength[i] = ReadShortArray(reader, rows);
												break;
											}
										}
										break;
									}
								}
							}

							for (int i = 0; i < textLength.Length; ++i) {
								if (textLength[i].Length == rows) {
									_textData[i] = new List<string>(rows);
									for (int r = 0; r < textLength[i].Length; ++r) {
										_textData[i].Add(ReadString(reader, textLength[i][r]));
									}
								} else {
									_textData[i] = new List<string>(new string[rows]);
								}
							}

							for (int i = 0; i < _intData.Length; ++i) {
								if (_intData[i].Count < rows) {
									_intData[i] = new List<int>(new int[rows]);
								}
							}
							for (int i = 0; i < _boolData.Length; ++i) {
								if (_boolData[i].Count < rows) {
									_boolData[i] = new List<bool>(new bool[rows]);
								}
							}
							for (int i = 0; i < _floatData.Length; ++i) {
								if (_floatData[i].Count < rows) {
									_floatData[i] = new List<float>(new float[rows]);
								}
							}

							_heights.Clear();
							for (int i = 0; i < rows; ++i) {
								_heights.Add(DEFAULT_CONTENT_HEIGHT);
							}
						}
					}
				} catch(System.Exception e) {
					EditorUtility.DisplayDialog("Error", $"Fail load:{filePath}", "Ok");
					Debug.LogError(e);
				}
			}
		}

		private int[] ReadIntArray(BinaryReader reader, int count) {
			byte[] bytes = reader.ReadBytes(count * sizeof(int));
			return MemoryMarshal.Cast<byte, int>(bytes).ToArray();
		}

		private short[] ReadShortArray(BinaryReader reader, int count) {
			byte[] bytes = reader.ReadBytes(count * sizeof(short));
			return MemoryMarshal.Cast<byte, short>(bytes).ToArray();
		}

		private bool[] ReadBoolArray(BinaryReader reader, int count) {
			byte[] bytes = reader.ReadBytes(count * sizeof(bool));
			return MemoryMarshal.Cast<byte, bool>(bytes).ToArray();
		}

		private float[] ReadFloatArray(BinaryReader reader, int count) {
			byte[] bytes = reader.ReadBytes(count * sizeof(float));
			return MemoryMarshal.Cast<byte, float>(bytes).ToArray();
		}

		private string ReadString(BinaryReader reader, short length) {
			byte[] bytes = reader.ReadBytes(length);
			return System.Text.Encoding.UTF8.GetString(bytes, 0, length);
		}

		private void DrawBackground() {
			_background.horizontalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
			_background.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;

			Color backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			Color borderColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

			Color headerBackgroundColor = new Color(0f, 0f, 0f, 1f);
			Color headerBorderColor = new Color(1f,1f,1f,1f);
			Color headerTextColor = new Color(1f,1f,1f,1f);

			float backgroundWidth = 0f;
			foreach(float w in _widths) { backgroundWidth += w; }

			_background.contentContainer.style.width = backgroundWidth + ROW_HEADER_WIDTH;

			_gridArea = new Box();
			_gridArea.style.position = Position.Absolute;
			_gridArea.style.left = ROW_HEADER_WIDTH;
			_gridArea.style.marginTop = COLUMN_HEADER_HEIGHT;
			_gridArea.style.marginBottom = CONTROL_PANEL_HEIGHT;
			_background.contentContainer.Add(_gridArea);

			_gridArea.style.width = backgroundWidth;

			_gridArea.style.backgroundColor = backgroundColor;

			_gridArea.style.borderLeftWidth = BORDER_WIDTH;
			_gridArea.style.borderTopWidth = BORDER_WIDTH;

			_gridArea.style.borderLeftColor = borderColor;
			_gridArea.style.borderTopColor = borderColor;

			_gridArea.RegisterCallback<MouseDownEvent>(evt => {
				Vector2 pos = evt.localMousePosition;
				int column = -1, row = -1;
				for (int i = 0; i < _widths.Count; ++i) {
					if (pos.x < _widths[i]) {
						column = i;
						break;
					}
					pos.x -= _widths[i];
				}

				for (int i = 0; i < _heights.Count; ++i) {
					if (pos.y < _heights[i]) {
						row = i;
						break;
					}
					pos.y -= _heights[i];
				}
				if (row >= 0 && column >= 0) {
					OnClick(row, column);
				}
			});

			_foreground = new Box();
			_foreground.pickingMode = PickingMode.Ignore;
			_foreground.style.flexDirection = FlexDirection.Column;
			_foreground.style.position = Position.Absolute;
			_foreground.style.width = backgroundWidth;
			_foreground.style.marginLeft = ROW_HEADER_WIDTH;
			_foreground.style.marginTop = COLUMN_HEADER_HEIGHT;
			_foreground.style.backgroundColor = new Color(0f,0f,0f,0f);
			_background.contentContainer.Add(_foreground);

			_rowNumberArea = new Box();
			_rowNumberArea.style.position = Position.Absolute;
			_rowNumberArea.style.backgroundColor = headerBackgroundColor;
			_rowNumberArea.style.left = _background.scrollOffset.x;
			_rowNumberArea.style.marginTop = COLUMN_HEADER_HEIGHT;
			_rowNumberArea.style.marginBottom = CONTROL_PANEL_HEIGHT;
			_rowNumberArea.style.width = ROW_HEADER_WIDTH;
			_background.contentContainer.Add(_rowNumberArea);

			_columnNumberArea = new Box();
			_columnNumberArea.style.position = Position.Absolute;
			_columnNumberArea.style.backgroundColor = headerBackgroundColor;
			_columnNumberArea.style.left = ROW_HEADER_WIDTH;
			_columnNumberArea.style.top = _background.scrollOffset.y;
			_columnNumberArea.style.width = backgroundWidth;
			_columnNumberArea.style.height = COLUMN_HEADER_HEIGHT;
			_background.contentContainer.Add(_columnNumberArea);

			Box leftTop = new Box();
			leftTop.style.position = Position.Absolute;
			leftTop.style.backgroundColor = headerBackgroundColor;
			leftTop.style.left = _background.scrollOffset.x;
			leftTop.style.top = _background.scrollOffset.y;
			leftTop.style.width = ROW_HEADER_WIDTH;
			leftTop.style.height = COLUMN_HEADER_HEIGHT;
			leftTop.style.borderRightWidth = 1f;
			leftTop.style.borderBottomWidth = 1f;
			leftTop.style.borderRightColor = headerBorderColor;
			leftTop.style.borderBottomColor = headerBorderColor;
			_background.contentContainer.Add(leftTop);

			_controlPanel = new Row().Height(CONTROL_PANEL_HEIGHT).Padding(horizontal: 12f);
			_controlPanel.style.position = Position.Absolute;
			_controlPanel.style.backgroundColor = headerBackgroundColor;
			_controlPanel.style.width = new Length(100, LengthUnit.Percent);
			_controlPanel.style.height = CONTROL_PANEL_HEIGHT;
			_controlPanel.style.left = 0f;
			// contentViewportのGenmetryChangedEventで初期化
			_controlPanel.style.top = -CONTROL_PANEL_HEIGHT;
			_background.contentContainer.Add(_controlPanel);

			_background.horizontalScroller.valueChanged += _ => {
				leftTop.style.left = _background.scrollOffset.x;
				_rowNumberArea.style.left = _background.scrollOffset.x;
				_controlPanel.style.left = _background.scrollOffset.x;
			};

			_background.verticalScroller.valueChanged += _ => {
				leftTop.style.top = _background.scrollOffset.y;
				_columnNumberArea.style.top = _background.scrollOffset.y;
				_controlPanel.style.top = _background.scrollOffset.y + _background.contentViewport.resolvedStyle.height - CONTROL_PANEL_HEIGHT;
			};

			_background.contentViewport.RegisterCallback<GeometryChangedEvent>(evt => {
				_controlPanel.style.width = _background.contentViewport.resolvedStyle.width;
				_controlPanel.style.top = _background.scrollOffset.y + _background.contentViewport.resolvedStyle.height - CONTROL_PANEL_HEIGHT;
			});

			_background.RegisterCallback<GeometryChangedEvent>(evt => {
				_background.contentContainer.style.minHeight = _background.resolvedStyle.height - _background.horizontalScroller.resolvedStyle.height;				
			});

			float currentWidth = 0f;
			for(int i = 0; i < _widths.Count; ++i) {
				Label header = new Label(_columns[i].Name);
				header.style.position = Position.Absolute;
				header.style.color = headerTextColor;
				header.style.left = currentWidth;
				header.style.top = 0f;
				header.style.width = _widths[i];
				header.style.height = COLUMN_HEADER_HEIGHT;
				header.style.unityTextAlign = TextAnchor.MiddleCenter;
				_columnNumberArea.Add(header);

				currentWidth += _widths[i];
				VisualElement line = new VisualElement();
				line.style.position = Position.Absolute;
				line.style.left = currentWidth;
				line.style.top = 0f;
				line.style.bottom = 0f;
				line.style.width = BORDER_WIDTH;
				line.style.backgroundColor = borderColor;
				_gridArea.Add(line);

				VisualElement headerLine = new VisualElement();
				headerLine.style.position = Position.Absolute;
				headerLine.style.left = currentWidth;
				headerLine.style.top = 0f;
				headerLine.style.width = BORDER_WIDTH;
				headerLine.style.height = COLUMN_HEADER_HEIGHT;
				headerLine.style.backgroundColor = headerBorderColor;
				_columnNumberArea.Add(headerLine);
			}

			Button addButton = new Button(clickEvent: () => {
				OnAddRow();
				foreach(List<int> intList in _intData) { intList.Add(default); }
				foreach(List<bool> boolList in _boolData) { boolList.Add(default); }
				foreach(List<float> floatList in _floatData) { floatList.Add(default); }
				foreach(List<string> textList in _textData) { textList.Add(default); }
				_heights.Add(DEFAULT_CONTENT_HEIGHT);
				UpdateBackground();
				DrawRow(_heights.Count-1);
			});
			float addButtonHeight = CONTROL_PANEL_HEIGHT * 0.75f;
			float addButtonMargin = (CONTROL_PANEL_HEIGHT - addButtonHeight) * 0.5f;
			addButton.style.height = addButtonHeight;
			addButton.style.width = addButtonHeight;
			addButton.style.marginTop = addButtonMargin;
			addButton.style.marginBottom = addButtonMargin;
			addButton.style.borderTopLeftRadius = addButtonHeight * 0.5f;
			addButton.style.borderTopRightRadius = addButtonHeight * 0.5f;
			addButton.style.borderBottomLeftRadius = addButtonHeight * 0.5f;
			addButton.style.borderBottomRightRadius = addButtonHeight * 0.5f;
			addButton.text = "+";

			_controlPanel.AddChildren(
				new Spacer().Weight(1f),
				addButton
			);

			UpdateBackground();
		}

		private void UpdateBackground() {
			Color borderColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
			Color headerBorderColor = new Color(1f,1f,1f,1f);
			Color headerTextColor = new Color(1f,1f,1f,1f);

			float backgroundHeight = 0f;
			foreach(float h in _heights) { backgroundHeight += h; }

			_background.contentContainer.style.height = backgroundHeight + COLUMN_HEADER_HEIGHT + CONTROL_PANEL_HEIGHT;
			_gridArea.style.height = backgroundHeight;

			foreach(VisualElement line in _horizontalLines) {
				_gridArea.Remove(line);
			}
			_horizontalLines.Clear();

			_rowNumberArea.style.height = backgroundHeight;
			_rowNumberArea.Clear();

			float currentHeight = 0f;
			for(int i = 0; i < _heights.Count; ++i) {
				int index = i;
				Label header = new Label((i+1).ToString());
				header.style.position = Position.Absolute;
				header.style.color = headerTextColor;
				header.style.left = 0f;
				header.style.right = 0f;
				header.style.top = currentHeight;
				header.style.height = _heights[i];
				header.style.unityTextAlign = TextAnchor.MiddleCenter;
				header.RegisterCallback<MouseDownEvent>(evt => {
					if (evt.button == 1) {
						GenericMenu menu = new GenericMenu();
						menu.AddItem(new GUIContent("Delete"), true, () => { RemoveRow(index); });
						menu.DropDown(new Rect(evt.mousePosition, Vector2.zero));
					}
				});
				_rowNumberArea.Add(header);

				currentHeight += _heights[i];
				VisualElement line = new VisualElement();
				line.style.position = Position.Absolute;
				line.style.left = 0f;
				line.style.right = 0f;
				line.style.top = currentHeight;
				line.style.height = BORDER_WIDTH;
				line.style.backgroundColor = borderColor;
				_gridArea.Add(line);
				_horizontalLines.Add(line);

				VisualElement headerLine = new VisualElement();
				headerLine.style.position = Position.Absolute;
				headerLine.style.left = 0f;
				headerLine.style.top = currentHeight;
				headerLine.style.width = ROW_HEADER_WIDTH;
				headerLine.style.height = BORDER_WIDTH;
				headerLine.style.backgroundColor = headerBorderColor;
				_rowNumberArea.Add(headerLine);
			}
		}

		private void OnClick(int row, int column) {
			if (_editField != null) {
				_background.contentContainer.Remove(_editField);
			}

			switch(_columns[column].Type) {
				case ColumnType.INT: {
					IntegerField intField = new IntegerField();
					_editField = intField;
					break;
				}
				case ColumnType.BOOL: {
					Toggle toggle = new Toggle();
					Box box = new Box();
					box.Add(toggle);
					box.style.justifyContent = Justify.Center;
					box.style.alignItems = Align.Center;
					_editField = box;
					break;
				}
				case ColumnType.FLOAT: {
					FloatField floatField = new FloatField();
					_editField = floatField;
					break;
				}
				case ColumnType.STRING: {
					TextField textField = new TextField();
					_editField = textField;
					break;
				}
			}

			float left = ROW_HEADER_WIDTH;
			for (int i = 0; i < column; ++i) {
				left += _widths[i];
			}
			float width = _widths[column];

			float top = COLUMN_HEADER_HEIGHT;
			for (int i = 0; i < row; ++i) {
				top += _heights[i];
			}
			float height = _heights[row];

			Color borderColor = new Color(0.5f, 0.6f, 1f);
			_editField.style.marginLeft = 0f;
			_editField.style.marginRight = 0f;
			_editField.style.marginTop = 0f;
			_editField.style.marginBottom = 0f;

			_editField.style.position = Position.Absolute;
			_editField.style.left = (int)left+1;
			_editField.style.top = (int)top+1;
			_editField.style.width = width;
			_editField.style.height = height;

			VisualElement innerDesign = _editField.Q("unity-text-input") ?? _editField;
			innerDesign.style.backgroundImage = Background.FromTexture2D(null);
			innerDesign.style.backgroundColor = new Color(1f,1f,1f,1f);
			innerDesign.style.marginLeft = 0f;
			innerDesign.style.marginRight = 0f;
			innerDesign.style.marginTop = 0f;
			innerDesign.style.marginBottom = 0f;
			innerDesign.style.color = Color.black;

			innerDesign.style.borderTopLeftRadius = 0f;
			innerDesign.style.borderTopRightRadius = 0f;
			innerDesign.style.borderBottomLeftRadius = 0f;
			innerDesign.style.borderBottomRightRadius = 0f;

			innerDesign.style.borderLeftColor = borderColor;
			innerDesign.style.borderRightColor = borderColor;
			innerDesign.style.borderTopColor = borderColor;
			innerDesign.style.borderBottomColor = borderColor;

			_background.contentContainer.Add(_editField);
		}

		private void RemoveRow(int index) {
			foreach(List<int> intList in _intData) { intList.RemoveAt(index); }
			foreach(List<bool> boolList in _boolData) { boolList.RemoveAt(index); }
			foreach(List<float> floatList in _floatData) { floatList.RemoveAt(index); }
			foreach(List<string> textList in _textData) { textList.RemoveAt(index); }
			_heights.RemoveAt(index);

			_foreground.Remove(_foregroundViews[index]);
			_foregroundViews.RemoveAt(index);

			UpdateBackground();
		}

		private void OnAddRow() {
			Row row = new Row()
				.Height(DEFAULT_CONTENT_HEIGHT);
			row.pickingMode = PickingMode.Ignore;
			for (int c = 0; c < _columns.Length; ++c) {
				Label label = new Label();
				label.style.color = Color.black;
				label.style.top = 0f;
				label.style.bottom = 0f;
				label.style.width = _widths[c];
				label.style.unityTextAlign = TextAnchor.MiddleCenter;
				label.pickingMode = PickingMode.Ignore;
				switch(_columns[c].Type) {
					case ColumnType.INT: { label.style.unityTextAlign = TextAnchor.MiddleRight; break; }
					case ColumnType.BOOL: { label.style.unityTextAlign = TextAnchor.MiddleCenter; break; }
					case ColumnType.FLOAT: { label.style.unityTextAlign = TextAnchor.MiddleRight; break; }
					case ColumnType.STRING: { label.style.unityTextAlign = TextAnchor.MiddleLeft; break; }
				}
				row.Add(label);
			}
			_foreground.Add(row);
			_foregroundViews.Add(row);
		}

		private void DrawRow(int row) {
			int intIdx = 0, boolIdx = 0, floatIdx = 0, textIdx = 0;

			string[] values = new string[_columns.Length];
			for(int c = 0; c < _columns.Length; ++c) {
				int id = _columns[c].Id;
				if (intIdx < _intIds.Length && _intIds[intIdx] == id) {
					values[c] = _intData[intIdx][row].ToString();
					intIdx++;
					continue;
				} else if (boolIdx < _boolIds.Length && _boolIds[boolIdx] == id) {
					values[c] = _boolData[boolIdx][row].ToString();
					boolIdx++;
					continue;
				} else if (floatIdx < _floatIds.Length && _floatIds[floatIdx] == id) {
					values[c] = _floatData[floatIdx][row].ToString();
					floatIdx++;
					continue;
				} else if (textIdx < _textIds.Length && _textIds[textIdx] == id) {
					values[c] = _textData[textIdx][row];
					textIdx++;
					continue;
				}
			}
			DrawRow(row, values);
		}

		private void DrawRow(int row, string[] values) {
			VisualElement view = _foregroundViews[row];

			List<Label> labels = view.Query<Label>().ToList();

			for (int i = 0; i < _columns.Length; ++i) {
				Label label = labels[i];
				label.text = values[i];
			}
		}
	}
}