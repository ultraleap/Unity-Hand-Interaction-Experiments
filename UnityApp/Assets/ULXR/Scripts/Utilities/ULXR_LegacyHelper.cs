using System.Collections;
using UnityEngine;

/// <summary>
/// Solves problems when using ULXR with older versions of the plugin
/// </summary>
public class ULXR_LegacyHelper : MonoBehaviour
{
    private SpriteRenderer _pointer0, _pointer1;
    private Color _transparent = new Color(0, 0, 0, 0);

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        // Wait a frame for the cursors to be instantiated before finding them
        yield return null;

        Transform leapEventSystem = transform.Find("LeapEventSystem");
        if (leapEventSystem != null)
        {
            _pointer0 = leapEventSystem.Find("Pointer 0").GetComponent<SpriteRenderer>();
            _pointer1 = leapEventSystem.Find("Pointer 1").GetComponent<SpriteRenderer>();
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        HideCursors();
    }

    /// <summary>
    /// This hides the instantiated cursors from LeapInputModule.cs
    /// Cursors may get added by default on import - this ensures that they don't appear.
    /// We can't delete them as this causes errors, and we can't reference the old input module to stop them appearing,
    /// as there's not an elegant way to detect which version we're on with an ifdef
    /// </summary>
    void HideCursors()
    {
        if (_pointer0 != null)
        {
            _pointer0.color = _transparent;
        }

        if (_pointer1 != null)
        {
            _pointer1.color = _transparent;
        }
    }
}
