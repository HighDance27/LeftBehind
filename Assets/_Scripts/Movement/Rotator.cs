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
            //Nếu tính tay hàm Atan2:
            // BƯỚC 1: Tính độ chênh lệch giữa a và b
            // deltaY = a.y - b.y | deltaX = a.x - b.x

            // BƯỚC 2: xét dấu của cả x và y sau khi trừ:
            // Nếu x > 0: Góc = Tan-1(y/x)
            // Nếu x < 0: Góc = Tan-1(y/x) + 180° (khi y >= 0) hoặc - 180° (khi y < 0)
            // Nếu x = 0: Góc = 90° (y > 0) hoặc -90° (y < 0)

            // BƯỚC 3: Nhân với Mathf.Rad2Deg (180/Pi ≈ 57.29) để đổi từ Radian sang Độ (-180 đến 180)
            return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;

        }
    }
}
