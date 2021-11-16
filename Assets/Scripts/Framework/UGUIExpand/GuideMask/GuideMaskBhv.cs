using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 新手引导动画
/// </summary>
[RequireComponent(typeof(EventPermeate))]
public class GuideMaskBhv : MonoBehaviour
{
    public GameObject target;

    private Material material;
    private float diameter; // 直径
    private float current = 0f;

    Vector3[] corners = new Vector3[4];
    float yVelocity = 0f;

    /// <summary>
    /// 执行引导
    /// </summary>
    /// <param name="target">模板对象</param>
    public void DoGuide(GameObject target)
    {
        if(null != target)
            this.target = target;
        // 设置事件透传对象
        gameObject.GetComponent<EventPermeate>().target = this.target;

        var canvas = GlobalObjs.s_canvas;
        target.GetComponent<RectTransform>().GetWorldCorners(corners);
        diameter = Vector2.Distance(WordToCanvasPos(canvas, corners[0]), WordToCanvasPos(canvas, corners[2])) / 2f;

        float x = corners[0].x + ((corners[3].x - corners[0].x) / 2f);
        float y = corners[0].y + ((corners[1].y - corners[0].y) / 2f);

        Vector3 center = new Vector3(x, y, 0f);
        Vector2 position = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, center, canvas.GetComponent<Camera>(), out position);

        center = new Vector4(position.x, position.y, 0f, 0f);
        material = GetComponent<Image>().material;
        material.SetVector("_Center", center);

        (canvas.transform as RectTransform).GetWorldCorners(corners);
        for (int i = 0; i < corners.Length; i++)
        {
            current = Mathf.Max(Vector3.Distance(WordToCanvasPos(canvas, corners[i]), center), current);
        }

        material.SetFloat("_Silder", current);
    }


    void Update()
    {
        float value = Mathf.SmoothDamp(current, diameter, ref yVelocity, 0.3f);
        if (!Mathf.Approximately(value, current))
        {
            current = value;
            material.SetFloat("_Silder", current);
        }
    }

    Vector2 WordToCanvasPos(Canvas canvas, Vector3 world)
    {
        Vector2 position = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, world, canvas.GetComponent<Camera>(), out position);
        return position;
    }
}
