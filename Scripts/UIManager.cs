using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class UIManager : MonoBehaviour
{

    #region 记录扫描框的屏幕坐标

    private Vector2 topLeft;
    private Vector2 topRight;
    private Vector2 bottomLeft;
    private Vector2 bottomRight;

    private void GetUIScreenPosition()
    {
        RectTransform rectTrans = transform.GetChild(0) as RectTransform;
        float uiWidth = rectTrans.sizeDelta.x;
        float uiHeight = rectTrans.sizeDelta.y;

        float xLeft = (Screen.width - uiWidth) * 0.5f * ratio;
        float xRight = Screen.width - xLeft;
        float yDown = (Screen.height - uiHeight) * 0.5f * ratio;
        float yUp = Screen.height - yDown;

        topLeft = new Vector2(xLeft, yUp);
        topRight = new Vector2(xRight, yUp);
        bottomLeft = new Vector2(xLeft, yDown);
        bottomRight = new Vector2(xRight, yDown);
    }

    #endregion

    #region 记录面片的屏幕坐标
    private Vector2 planeTopLeft;
    private Vector2 planeTopRight;
    private Vector2 planeBottomLeft;
    private Vector3 planeBottomRight;

    //记录面片的世界坐标
    private Vector3 planeTopLeft3D;
    private Vector3 planeTopRight3D;
    private Vector3 planeBottomLeft3D;
    private Vector3 planeBottomRight3D;

    private void GetPlaneScreenPosition()
    {
        //获取面片长宽
        float xOffset = planeMeshFilter.mesh.bounds.size.x * 0.5f * 10;
        float zOffset = planeMeshFilter.mesh.bounds.size.z * 0.5f * 10;

        //获取面片四点的世界坐标
        Vector3 imageTargetPos = planeMeshFilter.transform.parent.position;
        planeTopLeft3D = imageTargetPos + new Vector3(-xOffset, 0.01f, zOffset);
        planeTopRight3D = imageTargetPos + new Vector3(xOffset, 0.01f, zOffset);
        planeBottomLeft3D = imageTargetPos + new Vector3(-xOffset, -zOffset);
        planeBottomRight3D = imageTargetPos + new Vector3(xOffset, -zOffset);

        //把世界坐标转换成屏幕坐标
        planeTopLeft = Camera.main.WorldToScreenPoint(planeTopLeft3D);
        planeTopRight = Camera.main.WorldToScreenPoint(planeTopRight3D);
        planeBottomLeft = Camera.main.WorldToScreenPoint(planeBottomLeft3D);
        planeBottomRight = Camera.main.WorldToScreenPoint(planeBottomRight3D);
    }

    #endregion

    public Material[] mats;
    private MeshRenderer planeRender;
    private MeshFilter planeMeshFilter;
    private CanvasScaler scaler;
    //UI自适应缩放比例
    private float ratio;
    private Texture2D textureShot;
    public MeshRenderer earthRender;
    public MeshRenderer frameRender;
    public MeshRenderer earthARender;
    public GameObject uiSuccess;
    private bool isDetected;
    private bool isOutOfView;

    void Start () 
	{
        planeRender = GameObject.FindGameObjectWithTag("Plane").GetComponent<MeshRenderer>();
        planeMeshFilter = GameObject.FindGameObjectWithTag("Plane").GetComponent<MeshFilter>();
        uiSuccess.SetActive(false);

        scaler = gameObject.GetComponent<CanvasScaler>();
        ratio = Screen.width / scaler.referenceResolution.x;
        textureShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        //设置对焦
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

        GetUIScreenPosition();
	}

    private void SetShaderVector4(MeshRenderer render)
    {
        render.sharedMaterial.SetVector("_Uvpoint1", new Vector4(planeTopLeft3D.x, planeTopLeft3D.y, planeTopLeft3D.z, 1f));
        render.sharedMaterial.SetVector("_Uvpoint2", new Vector4(planeBottomLeft3D.x, planeBottomLeft3D.y, planeBottomLeft3D.z, 1f));
        render.sharedMaterial.SetVector("_Uvpoint3", new Vector4(planeTopRight3D.x, planeTopRight3D.y, planeTopRight3D.z, 1f));
        render.sharedMaterial.SetVector("_Uvpoint4", new Vector4(planeBottomRight3D.x, planeBottomRight3D.y, planeBottomRight3D.z, 1f));
    }

    private void ScreenShotMethod()
    {
        SetShaderVector4(earthRender);
        SetShaderVector4(frameRender);
        SetShaderVector4(earthARender);

        //获取截图时GPU的投影矩阵
        Matrix4x4 matrixGPU = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false);
        //获取截图时世界坐标到相机的矩阵
        Matrix4x4 matrixCamera = Camera.main.worldToCameraMatrix;
        Matrix4x4 matrixDot = matrixGPU * matrixCamera;

        earthRender.sharedMaterial.SetMatrix("_VP", matrixDot);
        frameRender.sharedMaterial.SetMatrix("_VP", matrixDot);
        earthARender.sharedMaterial.SetMatrix("_VP", matrixDot);

        //第一个"0,0"获取屏幕像素的起始点，第二个"0,0"填充texture2D时的坐标
        textureShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        textureShot.Apply();

        earthRender.sharedMaterial.mainTexture = textureShot;
        frameRender.sharedMaterial.mainTexture = textureShot;
        earthARender.sharedMaterial.mainTexture = textureShot;

        planeRender.gameObject.SetActive(false);
    }

    /// <summary>
    /// 判断TargetImage是否在屏幕中
    /// </summary>
    private bool IsPlaneInScanScreen()
    {
        return planeTopLeft.x > topLeft.x && planeTopLeft.y < topLeft.y &&
            planeTopRight.x < topRight.x && planeTopRight.y < topRight.y &&
            planeBottomLeft.x > bottomLeft.x && planeBottomLeft.y > bottomLeft.y &&
            planeBottomRight.x < bottomRight.x && planeBottomRight.y > bottomRight.y;
    }

    private IEnumerator UIDetectionSuccess()
    {
        yield return new WaitForSeconds(0.5f);
        uiSuccess.SetActive(true);
        planeRender.material = mats[0];
    }

    private IEnumerator ScreenShot()
    {
        yield return new WaitForSeconds(2);

        if (isDetected)
        {
            planeRender.material = mats[0];
            ScreenShotMethod();
        }
    }

	void Update () 
	{
        GetPlaneScreenPosition();

        //targetImage在扫描框内
        if(IsPlaneInScanScreen())
        {
            isOutOfView = false;
            if (!isDetected)
            {
                isDetected = true;
                //绿色材质
                planeRender.material = mats[1];
                StartCoroutine(UIDetectionSuccess());
                StartCoroutine(ScreenShot());
            }
        }
        else//不在扫描框内
        {
            if (!isOutOfView)
            {
                //红色材质
                planeRender.material = mats[2];
                isDetected = false;
                isOutOfView = true;
            }
        }
	}

    public void Quit()
    {
        Application.Quit();
    }
}
