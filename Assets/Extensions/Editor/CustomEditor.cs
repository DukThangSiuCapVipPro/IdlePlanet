using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[InitializeOnLoad]
public static class AutoAddButtonComponent
{
    static AutoAddButtonComponent()
    {
        // Subscribe to component addition callback
        ObjectFactory.componentWasAdded += OnComponentAdded;
    }

    private static void OnComponentAdded(Component component)
    {
        // Check if a Button was added
        if (component is Button button)
        {
            // See if it already has your custom script
            if (button.GetComponent<SmoothButton>() == null)
            {
                Undo.AddComponent<SmoothButton>(button.gameObject);
            }
        }
    }
}