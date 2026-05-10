using UnityEngine;

public static class TrajectoryCalculator
{
    public static Vector2 CalculateLaunchForce(Vector2 startPos, Vector2 targetPos, float angleDeg) // 시작 위치, 목표 위치, 각도로 발사 힘을 계산하는 함수
    {
        float D = Mathf.Abs(targetPos.x - startPos.x);
        float h = targetPos.y - startPos.y;
        float theta = angleDeg * Mathf.Deg2Rad;
        float g = Mathf.Abs(Physics2D.gravity.y);

        float cosTheta = Mathf.Cos(theta);
        float tanTheta = Mathf.Tan(theta);

        float denominator = 2 * cosTheta * cosTheta * (D * tanTheta - h);
        if (denominator <= 0)
            return Vector2.zero;
        

        float velocity = Mathf.Sqrt((g * D * D) / denominator);

        float vx = velocity * Mathf.Cos(theta);
        float vy = velocity * Mathf.Sin(theta);

        return new Vector2(vx, vy);
    }
}
