using UnityEngine;
using UnityEditor; 

[CustomPropertyDrawer(typeof(Zdarzenie))]
public class ZdarzenieDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Zwiększamy wysokość do 3 linijek (Typ, Opcja, Waga)
        return EditorGUIUtility.singleLineHeight * 3 + 6;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty typ = property.FindPropertyRelative("typ");
        SerializedProperty wrogPrefab = property.FindPropertyRelative("wrogPrefab");
        SerializedProperty iloscExp = property.FindPropertyRelative("iloscExp");
        SerializedProperty potworPrefab = property.FindPropertyRelative("potworPrefab");
        SerializedProperty waga = property.FindPropertyRelative("waga"); // Pobieramy nową wagę

        // Linijka 1: Typ Zdarzenia
        Rect typRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(typRect, typ);

        // Linijka 2: Zależna od typu (Wróg/Skarb/Potwór/Exp)
        Rect poleRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

        switch (typ.enumValueIndex)
        {
            case (int)TypZdarzenia.Walka:
                EditorGUI.PropertyField(poleRect, wrogPrefab, new GUIContent("Wróg (Prefab)"));
                break;
            case (int)TypZdarzenia.Skarb:
                EditorGUI.LabelField(poleRect, "Losuje 2 przedmioty ze wspólnej puli");
                break;
            case (int)TypZdarzenia.Doswiadczenie:
                EditorGUI.PropertyField(poleRect, iloscExp, new GUIContent("Ilość EXP"));
                break;
            case (int)TypZdarzenia.Potwor:
                EditorGUI.PropertyField(poleRect, potworPrefab, new GUIContent("Potwór (Prefab)"));
                break;
        }

        // Linijka 3: Waga losowania
        Rect wagaRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight * 2) + 4, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(wagaRect, waga, new GUIContent("Waga (Szansa)"));

        EditorGUI.EndProperty();
    }
}