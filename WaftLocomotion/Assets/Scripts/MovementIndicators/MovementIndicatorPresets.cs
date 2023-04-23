using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementIndicatorPresets : MonoBehaviour
{
    public HeadMovementIndicators headIndicators;
    public HandMovementIndicators leftIndicators, rightIndicators;

    public enum MovementIndicatorPreset { DEFAULT, MARIONETTE, DRAG_INDICATOR }

    public MovementIndicatorPreset currentPreset = MovementIndicatorPreset.DEFAULT;

    // Start is called before the first frame update
    void Awake()
    {
        SetPreset(currentPreset);
    }

    public void SetPreset(MovementIndicatorPreset preset)
    {
        switch(preset) {

            case MovementIndicatorPreset.MARIONETTE:
                SetMarionette();
                break;
            case MovementIndicatorPreset.DRAG_INDICATOR:
                SetDragIndicator();
                break;
            default:
            case MovementIndicatorPreset.DEFAULT:
                SetDefault();
                break;
        }
        currentPreset = preset;
    }

    private void SetMarionette()
    {
        leftIndicators.dotMagnitude = 3.2f;
        rightIndicators.dotMagnitude = 3.2f;

        headIndicators.dotMagnitude = 1.6f;
        headIndicators.headDotSecondaryMagnitude = 0.1f;

        leftIndicators.beamMagnitude = -1.6f;
        rightIndicators.beamMagnitude = -1.6f;
        headIndicators.beamMagnitude = 0.25f;

        leftIndicators.dotOn = true;
        rightIndicators.dotOn = true;

        leftIndicators.beamOn = true;
        rightIndicators.beamOn = true;
        leftIndicators.beamType = HandMovementIndicators.BeamType.PALM;
        rightIndicators.beamType = HandMovementIndicators.BeamType.PALM;

        leftIndicators.columnsOn = false;
        rightIndicators.columnsOn = false;

        headIndicators.headTargetOn = true;
        headIndicators.dotOn = false;
        headIndicators.dotSecondaryOn = false;
        headIndicators.beamOn = false;
        headIndicators.columnOn = true;        
    }

    private void SetDragIndicator()
    {
        leftIndicators.dotMagnitude = 3.2f;
        rightIndicators.dotMagnitude = 3.2f;

        headIndicators.dotMagnitude = 1.6f;
        headIndicators.headDotSecondaryMagnitude = 0.1f;

        leftIndicators.beamMagnitude = 0.25f;
        rightIndicators.beamMagnitude = 0.25f;
        headIndicators.beamMagnitude = -0.06f;

        leftIndicators.dotOn = false;
        rightIndicators.dotOn = false;

        leftIndicators.beamOn = false;
        rightIndicators.beamOn = false;

        leftIndicators.columnsOn = false;
        rightIndicators.columnsOn = false;

        headIndicators.headTargetOn = true;
        headIndicators.dotOn = true;
        headIndicators.dotSecondaryOn = false;
        headIndicators.beamOn = true;
        headIndicators.columnOn = false;
    }

    private void SetDefault()
    {
        leftIndicators.dotMagnitude = 3.2f;
        rightIndicators.dotMagnitude = 3.2f;

        headIndicators.dotMagnitude = 1.6f;
        headIndicators.headDotSecondaryMagnitude = 0.1f;

        leftIndicators.beamMagnitude = 0.25f;
        rightIndicators.beamMagnitude = 0.25f;
        headIndicators.beamMagnitude = 0.25f;

        leftIndicators.dotOn = false;
        rightIndicators.dotOn = false;

        leftIndicators.beamOn = true;
        rightIndicators.beamOn = true;
        leftIndicators.beamType = HandMovementIndicators.BeamType.PALM;
        rightIndicators.beamType = HandMovementIndicators.BeamType.PALM;

        leftIndicators.columnsOn = false;
        rightIndicators.columnsOn = false;

        headIndicators.headTargetOn = true;
        headIndicators.dotOn = true;
        headIndicators.dotSecondaryOn = false;
        headIndicators.beamOn = false;
        headIndicators.columnOn = false;
    }
}
