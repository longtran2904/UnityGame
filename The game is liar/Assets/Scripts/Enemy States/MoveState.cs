using UnityEngine;

[CreateAssetMenu(menuName = "Enemy States/Move State")]
public class MoveState : EnemyState
{
    public enum MoveType { Move, Jump, Fly, FlyAway, Curve };
    public MoveType moveType;
    public bool moveWhenInRange;

    private float timer;

    [ShowWhen("moveType", MoveType.Jump)] public float jumpForce;
    [ShowWhen("moveType", MoveType.Jump)] public float timeBtwJumps;
    private float timeBtwJumpsValue;

    private System.Action<Enemies> moveBehaviour;

    public override void Init(Enemies enemy)
    {
        switch (moveType)
        {
            case MoveType.Jump:
                timeBtwJumpsValue = timeBtwJumps;
                moveBehaviour = Jump;
                break;
            case MoveType.Curve:
                timer = 0;
                moveBehaviour = MoveInCurve;
                break;
            case MoveType.Fly:
                moveBehaviour = Fly;
                break;
            case MoveType.FlyAway:
                moveBehaviour = FlyAway;
                break;
            case MoveType.Move:
                moveBehaviour = Move;
                break;
        }
    }

    public override EnemyState UpdateState(Enemies enemy)
    {
        if ((moveWhenInRange && enemy.IsInRange()) || !moveWhenInRange)
        {
            moveBehaviour(enemy);
        }
        return nextState;
    }

    private void Move(Enemies enemy)
    {
        if (enemy.GroundCheck())
            enemy.rb.velocity = new Vector2(Mathf.Sign(enemy.player.transform.position.x - enemy.transform.position.x), 0) * enemy.speed * Time.deltaTime;
    }

    private void Jump(Enemies enemy)
    {
        if (Time.time > timeBtwJumpsValue && enemy.GroundCheck())
        {
            enemy.rb.velocity = new Vector2(MathUtils.Signed(enemy.player.transform.position.x - enemy.transform.position.x), 1).normalized * jumpForce;
            timeBtwJumpsValue = Time.time + timeBtwJumps;
        }
    }

    private void Fly(Enemies enemy)
    {
        enemy.rb.velocity = (enemy.player.transform.position - enemy.transform.position).normalized * enemy.speed * Time.deltaTime;
    }

    private void MoveInCurve(Enemies enemy)
    {
        timer = Mathf.Repeat(timer, 1);
        Vector2 point = MathUtils.GetBQCPoint(timer, enemy.transform.position, enemy.curvePoint.position, enemy.player.transform.position);
        enemy.rb.velocity = new Vector2(point.x - enemy.transform.position.x, point.y - enemy.transform.position.y).normalized * enemy.speed * Time.fixedDeltaTime;
        timer += Time.fixedDeltaTime;
    }

    private void FlyAway(Enemies enemy)
    {
        enemy.rb.velocity = -(enemy.player.transform.position - enemy.transform.position).normalized * enemy.speed * Time.deltaTime;
    }
}
