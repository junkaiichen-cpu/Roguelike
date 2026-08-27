using System.Numerics;

public static class EnemyPursuit
{
    public static Vector3 CalculateVelocity(Vector3 enemyPosition, Vector3 targetPosition, float movementSpeed)
    {
        if (movementSpeed <= 0)
        {
            return Vector3.Zero;
        }

        Vector3 offset = targetPosition - enemyPosition;
        float distance = offset.Length();
        if (distance == 0)
        {
            return Vector3.Zero;
        }

        Vector3 direction = distance > 1 ? offset / distance : offset;
        return direction * movementSpeed;
    }
}
