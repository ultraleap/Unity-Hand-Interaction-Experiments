using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshMakerManager : MonoBehaviour
{
    public GameObject mainMenuObject;
    public GameObject createMenuObject;
    public GameObject modifyMenuObject;
    public GameObject objectMenuObject;

    public MeshPainter meshPainter;
    public ObjectEditManager objManager;
    public ObjectCreator objCreator;

    public Material activeMaterial;
    public Material inactiveMaterial;

    private void Awake()
    {
        SetColorSlider(0.2f);
    }

    public void MainMenuMode()
    {
        DisableAllModes();
        mainMenuObject.SetActive(true);
    }

    void DisableAllModes()
    {
        meshPainter.canPaint = false;
        objCreator.canCreate = false;
        objManager.DisableInteraction();
        objManager.DisableScaling();

        mainMenuObject.SetActive(false);
        createMenuObject.SetActive(false);
        modifyMenuObject.SetActive(false);
        objectMenuObject.SetActive(false);
    }

    #region Create

    bool meshPaintEnabled = true;

    public MeshRenderer sliderRenderer;

    public MeshRenderer freeDrawBtnRend;
    public MeshRenderer sphereBtnRend;
    public MeshRenderer cubeBtnRend;
    public MeshRenderer cylinderBtnRend;


    public void ChangeModeCreate()
    {
        DisableAllModes();
        createMenuObject.SetActive(true);

        meshPainter.paintMode = MeshPaintMode.CREATE;
        meshPainter.canPaint = meshPaintEnabled;
    }

    public void SetColorSlider(float _hue)
    {
        Color newcolor = Color.HSVToRGB(_hue, 0.4f, 0.8f);
        sliderRenderer.material.color = newcolor;
        meshPainter.SetObjectColor(newcolor);
        objCreator.SetObjectColor(newcolor);
    }

    public void CreateModeDrawMesh()
    {
        meshPaintEnabled = true;
        meshPainter.canPaint = meshPaintEnabled;
        objCreator.canCreate = false;

        ChangeCreateObjectButtonMaterials(freeDrawBtnRend);
    }

    public void CreateModeCube()
    {
        meshPaintEnabled = false;
        meshPainter.canPaint = meshPaintEnabled;
        objCreator.canCreate = true;
        objCreator.objType = ObjectType.CUBE;

        ChangeCreateObjectButtonMaterials(cubeBtnRend);
    }

    public void CreateModeSphere()
    {
        meshPaintEnabled = false;
        meshPainter.canPaint = meshPaintEnabled;
        objCreator.canCreate = true;
        objCreator.objType = ObjectType.SPHERE;

        ChangeCreateObjectButtonMaterials(sphereBtnRend);
    }

    public void CreateModeCylinder()
    {
        meshPaintEnabled = false;
        meshPainter.canPaint = meshPaintEnabled;
        objCreator.canCreate = true;
        objCreator.objType = ObjectType.CYLINDER;

        ChangeCreateObjectButtonMaterials(cylinderBtnRend);
    }

    void ChangeCreateObjectButtonMaterials(MeshRenderer rendererToEnable)
    {
        freeDrawBtnRend.material = inactiveMaterial;
        sphereBtnRend.material = inactiveMaterial;
        cubeBtnRend.material = inactiveMaterial;
        cylinderBtnRend.material = inactiveMaterial;

        rendererToEnable.material = activeMaterial;
    }

    #endregion
    #region Modify

    public void ChangeModeModifyMesh()
    {
        DisableAllModes();
        modifyMenuObject.SetActive(true);

        meshPainter.paintMode = MeshPaintMode.MODIFY;
        meshPainter.canPaint = true;
    }

    #endregion
    #region Edit Object

    public MeshRenderer moveBtnRend;
    public MeshRenderer scaleBtnRend;

    bool interactMode = true;

    public void ChangeModeEditObject()
    {
        DisableAllModes();
        objectMenuObject.SetActive(true);

        if(interactMode)
        {
            objManager.EnableInteraction();
        }
        else
        {
            objManager.EnableScaling();
            objManager.EnableInteraction();
        }
    }

    public void SetInteractState()
    {
        interactMode = true;
        ChangeEditObjectButtonMaterials(moveBtnRend);
        objManager.DisableScaling();
    }

    public void SetScaleState()
    {
        interactMode = false;
        ChangeEditObjectButtonMaterials(scaleBtnRend);
        objManager.EnableScaling();
    }

    void ChangeEditObjectButtonMaterials(MeshRenderer rendererToEnable)
    {
        moveBtnRend.material = inactiveMaterial;
        scaleBtnRend.material = inactiveMaterial;

        rendererToEnable.material = activeMaterial;
    }

    #endregion

}