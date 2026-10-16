using UnityEditor;
using UnityEngine;

// Ten skrypt MUSI leżeć w folderze o nazwie "Editor" gdziekolwiek w Assets,
// np. Assets/Scripts/Editor/SpellShapeDrawer.cs
[CustomPropertyDrawer(typeof(SpellShape))]
public class SpellShapeDrawer : PropertyDrawer
{
    const int GridSize = 5;
    const float CellSize = 24f;
    const float Spacing = 2f;
    const float TopPadding = 20f;
    const float BottomPadding = 12f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty cellsProp = property.FindPropertyRelative("cells");

        // Nagłówek z nazwą pola (np. "Shape")
        Rect labelRect = new Rect(position.x, position.y, position.width, 18);
        EditorGUI.LabelField(labelRect, label);

        float gridPixelWidth = GridSize * (CellSize + Spacing) - Spacing;
        float startX = position.x + (position.width - gridPixelWidth) / 2f;
        float startY = position.y + TopPadding;

        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                int index = row * GridSize + col;
                SerializedProperty cellProp = cellsProp.GetArrayElementAtIndex(index);

                Rect cellRect = new Rect(
                    startX + col * (CellSize + Spacing),
                    startY + row * (CellSize + Spacing),
                    CellSize,
                    CellSize
                );

                Color prevColor = GUI.color;
                GUI.color = cellProp.boolValue
                    ? new Color(0.35f, 0.7f, 1f)   // zaznaczona komórka - niebieska
                    : new Color(0.22f, 0.22f, 0.22f); // pusta komórka - szara

                if (GUI.Button(cellRect, GUIContent.none))
                {
                    cellProp.boolValue = !cellProp.boolValue;
                }

                GUI.color = prevColor;
            }
        }

        // Przycisk czyszczący całą siatkę
        Rect clearButtonRect = new Rect(
            startX,
            startY + GridSize * (CellSize + Spacing) + 4,
            gridPixelWidth,
            18
        );
        if (GUI.Button(clearButtonRect, "Wyczyść kształt"))
        {
            for (int i = 0; i < cellsProp.arraySize; i++)
            {
                cellsProp.GetArrayElementAtIndex(i).boolValue = false;
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return TopPadding
             + GridSize * (CellSize + Spacing)
             + BottomPadding
             + 18; // miejsce na przycisk "Wyczyść kształt"
    }
}
