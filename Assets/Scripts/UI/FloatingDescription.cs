using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatingDescription : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descText;
    private const int maxWidth = 250;
    private RectTransform rect;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }
    public void SetDescription(Vector3 pos, string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            gameObject.SetActive(false);
            return;
        }

        /* wrapping�� Ǯ����� �� text content�� ����� üũ.
         * �ִ� ����� �����Ͽ� �׿� ���� height �߰� ����
         * �� �� �ٽ� wrapping�Ͽ� �ڽ��� �˸°� ������ ��.
         */

        descText.enableWordWrapping = false;
        descText.text = str;

        if (descText.preferredWidth > maxWidth)
        {
            float height = Mathf.CeilToInt(descText.preferredWidth / maxWidth) * descText.preferredHeight;
            rect.sizeDelta = new Vector2(maxWidth, height);
            descText.enableWordWrapping = true;
        }
        else
        {
            rect.sizeDelta = new Vector2(descText.preferredWidth, descText.preferredHeight);
        }

        MoveDescription(pos);
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }
    public void MoveDescription(Vector3 pos)
    {
        /* ������ ������ ���� �� Description�� ������ ���� �� ���ٸ� ��ġ����.
         * �������� �����ٸ� pos�� x�� rect��ŭ ��.
         * �Ʒ��� �����ٸ� pos�� y�� rect��ŭ ����.
         */

        Vector2 size = new Vector2(
                pos.x + rect.sizeDelta.x,
                pos.y - rect.sizeDelta.y);

        if (size.x > Screen.currentResolution.width) pos.x -= rect.sizeDelta.x;
        if (size.y > Screen.currentResolution.height) pos.y -= rect.sizeDelta.y;

        rect.anchoredPosition = pos;
    }
}
