using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GooglyEye : MonoBehaviour {

    public float radius = 1;
    public Transform pupil;

    Vector3 _pupilPos;
    Vector3 _pupilPosOld;
    float _interpolation;
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 pupilPosNew = 2 * _pupilPos - _pupilPosOld + 0.5f * Physics.gravity * Time.deltaTime * Time.deltaTime;

        pupilPosNew = transform.TransformPoint(ClampPupilToLocal(pupilPosNew));

        _pupilPosOld = _pupilPos;
        _pupilPos = pupilPosNew;

        _interpolation += Time.deltaTime;
	}

    private void Update()
    {
        _interpolation = Mathf.Clamp01((Time.deltaTime - _interpolation) / Time.fixedDeltaTime);

        Vector3 pupilPos = ClampPupilToLocal(Vector3.Lerp(_pupilPosOld, _pupilPos, _interpolation));

        pupil.localPosition = pupilPos;

        _interpolation = 0f;
    }

    Vector3 ClampPupilToLocal(Vector3 worldPos)
    {
        Vector3 local = transform.InverseTransformPoint(worldPos);
        local.z = 0f;
        if (local.sqrMagnitude > radius * radius)
            local = local.normalized * radius;

        return local;
    }

}
