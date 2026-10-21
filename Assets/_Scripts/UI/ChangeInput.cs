using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeInput : MonoBehaviour
{
    EventSystem system;
    public Selectable firstInput;
    public Button submitButton;

    private void Start()
    {
        system = EventSystem.current;
        firstInput.Select(); //tự focus vào inputfield đầu tiên
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //Tìm cái UI hiện đang được chọn. Sau đó tìm xem phía dưới nó có cái UI nào không
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null)
            {
                next.Select();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {

            submitButton.onClick.Invoke();
        }
    }
}
