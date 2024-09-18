/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2017.                                 *
 * Leap Motion proprietary and  confidential.                                 *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Leap.Unity.InputModuleAurora {
  public class ToggleToggler : MonoBehaviour {
    public Text text;
    public UnityEngine.UI.Image image;
    public Color OnColor;
    public Color OffColor;

    public void SetToggle(Toggle toggle) {
      if (toggle.isOn) {
        if (text) text.text = "On";
        if (text) text.color = Color.white;
        if (image) image.color = OnColor;
      } else {
        if (text) text.text = "Off";
        if (text) text.color = new Color(0.3f, 0.3f, 0.3f);
        if (image) image.color = OffColor;
      }
    }
  }
}
