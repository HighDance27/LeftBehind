using UnityEngine;

namespace TopDown.Movement
{

    public class Rotator : MonoBehaviour
    {
        protected void LookAt(Transform rotatedTransform, Vector3 target)
        {
            //tính góc xoay giữa transform và target
            float lookAngle = AngleBetweenTwoPoints(transform.position, target) - 90;//-90 để cho nhân vật nhìn đúng theo trỏ(sprite bị ngược)

            //gán góc xoay vừa tính vào trục Z của transform
            rotatedTransform.eulerAngles = new Vector3(0, 0, lookAngle);
        }

        private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
        {
            // Mathf.Atan2: Hàm lượng giác tính góc dựa trên sự chênh lệch tọa độ Y và X
            // Nó trả về giá trị đơn vị Radian, sau đó nhân với Mathf.Rad2Deg để đổi sang đơn vị Độ (0-360)
            return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
        }
    }
}
