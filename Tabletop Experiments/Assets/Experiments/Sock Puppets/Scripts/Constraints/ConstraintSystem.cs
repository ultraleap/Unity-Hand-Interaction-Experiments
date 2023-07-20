using UnityEngine;

public class ConstraintSystem : MonoBehaviour {

  public int refinementIteration = 50;
    //min ~ 10, 50 is sub mm refinement

    [SerializeField] private SphereCollider _headCollider;
  
  DistanceConstraint[] constraints;

	void Start () {
    constraints = GetComponentsInChildren<DistanceConstraint>();

        foreach (var constraint in constraints)
        {
            constraint.headCollider = _headCollider;
        }
  }

	void Update () {
    for (int i = 0; i < refinementIteration; i++) {
      foreach (DistanceConstraint constraint in constraints) {
        constraint.ResolveConstraint();
      }
    }

    foreach (DistanceConstraint constraint in constraints)
    {
       constraint.PostProcessConstraint();
    }

    }
}
