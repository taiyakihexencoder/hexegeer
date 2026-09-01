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

		[System.Serializable]
		public class Column {
			public int Id;
			public string Name;
			public ColumnType Type;

			public Column(int id, string name, ColumnType type) {
				Id = id;
				Name = name;
				Type = type;
			}
		}

		public enum ColumnType {
			INT,
			LONG,
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
		private int[] _longIds;
		private int[] _boolIds;
		private int[] _floatIds;
		private int[] _textIds;

		List<int>[] _intData = null;
		List<long>[] _longData = null;
		List<bool>[] _boolData = null;
		List<float>[] _floatData = null;
		List<string>[] _textData = null;

		private string _pathFromAssets;

		public EditorGridView(string pathFromAssets, params Column[] columns) {
			style.width = new Length(100, LengthUnit.Percent);
			style.height = new Length(100, LengthUnit.Percent);

			_pathFromAssets = pathFromAssets;
			_columns = columns;

			_widths = new List<float>();
			for (int i = 0; i < _columns.Length; ++i) {
				_widths.Add(100f);
			}

			_heights = new List<float>();

			List<int> intColumns = new List<int>();
			List<int> longColumns = new List<int>();
			List<int> boolColumns = new List<int>();
			List<int> floatColumns = new List<int>();
			List<int> textColumns = new List<int>();
			for (int c = 0; c < columns.Length; ++c) {
				switch(columns[c].Type) {
					case ColumnType.INT: {
						intColumns.Add(c);
						break;
					}
					case ColumnType.LONG: {
						longColumns.Add(c);
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

			_longIds = new int[longColumns.Count];
			_longData = new List<long>[longColumns.Count];
			for (int i = 0; i < longColumns.Count; ++i) {
				_longIds[i] = columns[longColumns[i]].Id;
				_longData[i] = new List<long>();
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

			Load(pathFromAssets);

			_background = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
			_background.StretchToParentSize();
			Add(_background);

			DrawBackground();

			for (int r = 0; r < _heights.Count; ++r) {
				OnAddRow(_heights[r]);
			}
			DrawRows();
		}

		private void Load(string pathFromAssets) {
			string filePath = Application.dataPath + Path.DirectorySeparatorChar + pathFromAssets;
			if (File.Exists(filePath)) {
				try {
					using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
						using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.UTF8)) {
							short columns = reader.ReadInt16();
							int rows = reader.ReadInt32();

							int[] ids = new int[columns];
							ColumnType[] types = new ColumnType[columns];
							float[] widths = new float[columns];

							for (short i = 0; i < columns; ++i) {
								ids[i] = reader.ReadInt32();
								types[i] = (ColumnType)(int)reader.ReadByte();
								widths[i] = reader.ReadSingle();
							}

							_heights = new List<float>(ReadFloatArray(reader, rows));

							for (short i = 0; i < columns; ++i) {
								for (short c = 0; c < _columns.Length; ++c) {
									if (_columns[c].Id == ids[i]) {
										if (_columns[c].Type == types[c]) { _widths[c] = widths[i]; }
										break;
									}
								}
							}

							short[][] textLength = new short[_textData.Length][];
							for (int i = 0; i < textLength.Length; ++i) {
								textLength[i] = new short[0];
							}

							for (int c = 0; c < columns; ++c) {
								switch(types[c]) {
									case ColumnType.INT: {
										int[] intList = ReadIntArray(reader, rows);
										for (int i = 0; i < _intIds.Length; ++i) {
											if (_intIds[i] == ids[c]) {
												_intData[i] = new List<int>(intList);
												break;
											}
										}
										break;
									}
									case ColumnType.LONG: {
										long[] longList = ReadLongArray(reader, rows);
										for (int i = 0; i < _longIds.Length; ++i) {
											if (_longIds[i] == ids[c]) {
												_longData[i] = new List<long>(longList);
												break;
											}
										}
										break;
									}
									case ColumnType.BOOL: {
										bool[] boolList = ReadBoolArray(reader, rows);
										for (int i = 0; i < _boolIds.Length; ++i) {
											if (_boolIds[i] == ids[c]) {
												_boolData[i] = new List<bool>(boolList);
												break;
											}
										}
										break;
									}
									case ColumnType.FLOAT: {
										float[] floatList = ReadFloatArray(reader, rows);
										for (int i = 0; i < _floatIds.Length; ++i) {
											if (_floatIds[i] == ids[c]) {
												_floatData[i] = new List<float>(floatList);
												break;
											}
										}
										break;
									}
									case ColumnType.STRING: {
										short[] textLengthList = ReadShortArray(reader, rows);
										for (int i = 0; i < _textIds.Length; ++i) {
											if (_textIds[i] == ids[c]) {
												textLength[i] = textLengthList;
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
							for (int i = 0; i < _longData.Length; ++i) {
								if (_longData[i].Count < rows) {
									_longData[i] = new List<long>(new long[rows]);
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

		private long[] ReadLongArray(BinaryReader reader, int count) {
			byte[] bytes = reader.ReadBytes(count * sizeof(long));
			return MemoryMarshal.Cast<byte, long>(bytes).ToArray();
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
			_gridArea.style.right = 0;
			_gridArea.style.marginTop = COLUMN_HEADER_HEIGHT;
			_gridArea.style.marginBottom = CONTROL_PANEL_HEIGHT;
			_background.contentContainer.Add(_gridArea);


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
			_rowNumberArea.style.top = COLUMN_HEADER_HEIGHT;
			_rowNumberArea.style.bottom = CONTROL_PANEL_HEIGHT;
			_rowNumberArea.style.width = ROW_HEADER_WIDTH;
			_background.contentContainer.Add(_rowNumberArea);

			_columnNumberArea = new Box();
			_columnNumberArea.style.position = Position.Absolute;
			_columnNumberArea.style.backgroundColor = headerBackgroundColor;
			_columnNumberArea.style.left = ROW_HEADER_WIDTH;
			_columnNumberArea.style.right = 0;
			_columnNumberArea.style.top = _background.scrollOffset.y;
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
				headerLine.style.height = COLUMN_HEADER_HEIGHT-1;
				headerLine.style.backgroundColor = headerBorderColor;
				_columnNumberArea.Add(headerLine);
			}

			Button saveButton = new Button(clickEvent: () => {
				try {
					Save();
				} catch (System.Exception e) {
					EditorUtility.DisplayDialog("Error", $"Save failed:{e.Message}", "Ok");
					Debug.LogError(e);
				}
			});
			saveButton.style.borderTopLeftRadius = 4f;
			saveButton.style.borderTopRightRadius = 4f;
			saveButton.style.borderBottomLeftRadius = 4f;
			saveButton.style.borderBottomRightRadius = 4f;
			saveButton.style.marginTop = 2f;
			saveButton.style.marginBottom = 2f;
			saveButton.style.paddingLeft = 12f;
			saveButton.style.paddingRight = 12f;
			saveButton.text = "Save";

			Button addButton = new Button(clickEvent: () => {
				OnAddRow();
				foreach(List<int> intList in _intData) { intList.Add(0); }
				foreach(List<long> longList in _longData) { longList.Add(0L); }
				foreach(List<bool> boolList in _boolData) { boolList.Add(false); }
				foreach(List<float> floatList in _floatData) { floatList.Add(0f); }
				foreach(List<string> textList in _textData) { textList.Add(""); }
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
				saveButton,
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
				headerLine.style.width = ROW_HEADER_WIDTH-1;
				headerLine.style.height = BORDER_WIDTH;
				headerLine.style.backgroundColor = headerBorderColor;
				_rowNumberArea.Add(headerLine);
			}
		}

		private void OnClick(int row, int column) {
			ShowEditField(row, column);
		}

		private void ShowEditField(int row, int column) {
			HideEditField();

			int id = _columns[column].Id;

			switch(_columns[column].Type) {
				case ColumnType.INT: {
					for (int i = 0; i < _intIds.Length; ++i) {
						if (_intIds[i] == id) {
							IntegerField intField = new IntegerField();
							intField.SetValueWithoutNotify(_intData[i][row]);
							int index = i;
							intField.RegisterValueChangedCallback(v => { 
								_intData[index][row] = v.newValue; 
								DrawRow(row);
							});
							_editField = intField;
							break;
						}
					}
					break;
				}
				case ColumnType.LONG: {
					for (int i = 0; i < _longIds.Length; ++i) {
						if (_longIds[i] == id) {
							LongField longField = new LongField();
							longField.SetValueWithoutNotify(_longData[i][row]);
							int index = i;
							longField.RegisterValueChangedCallback(v => { 
								_longData[index][row] = v.newValue; 
								DrawRow(row);
							});
							_editField = longField;
						}
					}
					break;
				}
				case ColumnType.BOOL: {
					for (int i = 0; i < _boolIds.Length; ++i) {
						if (_boolIds[i] == id) {
							Toggle toggle = new Toggle();
							toggle.SetValueWithoutNotify(_boolData[i][row]);
							int index = i;
							toggle.RegisterValueChangedCallback(v => { 
								_boolData[index][row] = v.newValue; 
								DrawRow(row);
							});
							Box box = new Box();
							box.Add(toggle);
							box.style.justifyContent = Justify.Center;
							box.style.alignItems = Align.Center;
							_editField = box;
						}
					}
					break;
				}
				case ColumnType.FLOAT: {
					for (int i = 0; i < _floatIds.Length; ++i) {
						if (_floatIds[i] == id) {
							FloatField floatField = new FloatField();
							floatField.SetValueWithoutNotify(_floatData[i][row]);
							int index = i;
							floatField.RegisterValueChangedCallback(v => { 
								_floatData[index][row] = v.newValue;
								DrawRow(row);
							});
							_editField = floatField;
						}
					}
					break;
				}
				case ColumnType.STRING: {
					for (int i = 0; i < _textIds.Length; ++i) {
						if (_textIds[i] == id) {
							TextField textField = new TextField();
							textField.SetValueWithoutNotify(_textData[i][row]);
							int index = i;
							textField.RegisterValueChangedCallback(v => {
								_textData[index][row] = v.newValue;
								DrawRow(row);
							});
							_editField = textField;
						}
					}
					break;
				}
			}

			if (_editField != null) {
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
		}

		private void HideEditField() {
			if (_background.contentContainer.Contains(_editField)) {
				_background.contentContainer.Remove(_editField);
				_editField = null;
			}
		}

		private void RemoveRow(int index) {
			HideEditField();

			foreach(List<int> intList in _intData) { intList.RemoveAt(index); }
			foreach(List<long> longList in _longData) { longList.RemoveAt(index); }
			foreach(List<bool> boolList in _boolData) { boolList.RemoveAt(index); }
			foreach(List<float> floatList in _floatData) { floatList.RemoveAt(index); }
			foreach(List<string> textList in _textData) { textList.RemoveAt(index); }
			_heights.RemoveAt(index);

			_foreground.Remove(_foregroundViews[index]);
			_foregroundViews.RemoveAt(index);

			UpdateBackground();
		}

		private void OnAddRow(float? height = null) {
			Row row = new Row()
				.Height(height ?? DEFAULT_CONTENT_HEIGHT);
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
					case ColumnType.LONG: { label.style.unityTextAlign = TextAnchor.MiddleRight; break; }
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
			int intIdx = 0, longIdx = 0, boolIdx = 0, floatIdx = 0, textIdx = 0;

			string[] values = new string[_columns.Length];
			for(int c = 0; c < _columns.Length; ++c) {
				int id = _columns[c].Id;
				if (intIdx < _intIds.Length && _intIds[intIdx] == id) {
					values[c] = _intData[intIdx][row].ToString();
					intIdx++;
					continue;
				} else if (longIdx < _longIds.Length && _longIds[longIdx] == id) {
					values[c] = _longData[longIdx][row].ToString();
					longIdx++;
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

		private void DrawRows() {
			int intIdx = 0, longIdx = 0, boolIdx = 0, floatIdx = 0, textIdx = 0;

			string[] values = new string[_columns.Length];
			List<string[]> valueList = new List<string[]>(_heights.Count);
			for (int row = 0; row < _heights.Count; ++row) {
				valueList.Add(new string[_columns.Length]);
			}

			for(int c = 0; c < _columns.Length; ++c) {
				int id = _columns[c].Id;
				if (intIdx < _intIds.Length && _intIds[intIdx] == id) {
					for (int row = 0; row < _heights.Count; ++row) {
						valueList[row][c] = _intData[intIdx][row].ToString();
					}
					intIdx++;
					continue;
				} else if (longIdx < _longIds.Length && _longIds[longIdx] == id) {
					for (int row = 0; row < _heights.Count; ++row) {
						valueList[row][c] = _longData[longIdx][row].ToString();
					}
					longIdx++;
					continue;
				} else if (boolIdx < _boolIds.Length && _boolIds[boolIdx] == id) {
					for (int row = 0; row < _heights.Count; ++row) {
						valueList[row][c] = _boolData[boolIdx][row].ToString();
					}
					boolIdx++;
					continue;
				} else if (floatIdx < _floatIds.Length && _floatIds[floatIdx] == id) {
					for (int row = 0; row < _heights.Count; ++row) {
						valueList[row][c] = _floatData[floatIdx][row].ToString();
					}
					floatIdx++;
					continue;
				} else if (textIdx < _textIds.Length && _textIds[textIdx] == id) {
					for (int row = 0; row < _heights.Count; ++row) {
						valueList[row][c] = _textData[textIdx][row];
					}
					textIdx++;
					continue;
				}
			}

			for (int row = 0; row < _heights.Count; ++row) {
				DrawRow(row, valueList[row]);
			}
		}

		public void Save() {
			string basePath = Application.dataPath;
			string[] paths = _pathFromAssets.Split(Path.DirectorySeparatorChar);
			foreach(string path in paths) {
				if (!Directory.Exists(basePath)) {
					Directory.CreateDirectory(basePath);
				}
				basePath += Path.DirectorySeparatorChar + path;
			}
			string absPath = Application.dataPath + Path.DirectorySeparatorChar + _pathFromAssets;

			using (FileStream stream = new FileStream(absPath, FileMode.OpenOrCreate, FileAccess.Write)) {
				using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8)) {
					// 列数
					writer.Write((short)_columns.Length);
					// 行数
					writer.Write(_heights.Count);

					// 列情報 id, type, width
					for (int c = 0; c < _columns.Length; ++c) {
						writer.Write(_columns[c].Id);
						writer.Write((byte)(int)_columns[c].Type);
						writer.Write(_widths[c]);
					}

					// 行情報
					writer.Write(MemoryMarshal.Cast<float, byte>(_heights.ToArray()).ToArray());

					List<byte[]>[] textData = new List<byte[]>[_textData.Length];
					List<short>[] textLengths = new List<short>[_textData.Length];
					for (int i = 0; i < _textData.Length; ++i) {
						textData[i] = new List<byte[]>(_textData[i].Count);
						textLengths[i] = new List<short>(_textData[i].Count);
						for (int j = 0; j < _textData[i].Count; ++j) {
							byte[] data = System.Text.Encoding.UTF8.GetBytes(_textData[i][j]);
							textData[i].Add(data);
							textLengths[i].Add((short)data.Length);
						}
					}

					// 各列のリスト
					int intIdx = 0, longIdx = 0, boolIdx = 0, floatIdx = 0, textIdx = 0;
					foreach (Column column in _columns) {
						int id = column.Id;
						if (intIdx < _intIds.Length && _intIds[intIdx] == id) {
							writer.Write(MemoryMarshal.Cast<int, byte>(_intData[intIdx].ToArray()).ToArray());
							intIdx++;
						} else if (longIdx < _longIds.Length && _longIds[longIdx] == id) {
							writer.Write(MemoryMarshal.Cast<long, byte>(_longData[longIdx].ToArray()).ToArray());
							longIdx++;
						} else if (boolIdx < _boolIds.Length && _boolIds[boolIdx] == id) {
							writer.Write(MemoryMarshal.Cast<bool, byte>(_boolData[boolIdx].ToArray()).ToArray());
							boolIdx++;
						} else if (floatIdx < _floatIds.Length && _floatIds[floatIdx] == id) {
							writer.Write(MemoryMarshal.Cast<float, byte>(_floatData[floatIdx].ToArray()).ToArray());
							floatIdx++;
						} else if (textIdx < _textIds.Length && _textIds[textIdx] == id) {
							writer.Write(MemoryMarshal.Cast<short, byte>(textLengths[textIdx].ToArray()).ToArray());
							textIdx++;
						}
					}

					foreach(List<byte[]> texts in textData) {
						foreach(byte[] text in texts) {
							writer.Write(text);
						}
					}
				}
			}
		}
	}
}