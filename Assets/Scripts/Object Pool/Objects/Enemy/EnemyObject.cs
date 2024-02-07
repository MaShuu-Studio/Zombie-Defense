using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Poolable/Enemy (Poolable)")]
public class EnemyObject : BTPoolable,
    IDamagedObject, IAttackObject, IMovingObject, IBuffTargetObject
{
    [Space]
    [SerializeField] private ObjectHpBar hpBar;

    [Space]
    [SerializeField] private Rigidbody2D rigidbody;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Space]
    [SerializeField] private Pathfinding.AIPath aiPath;
    [SerializeField] private Pathfinding.AIDestinationSetter aIDestinationSetter;

    Enemy data;

    private int hp;
    private int maxhp;
    private Collider2D targetCollider;
    private int dmg;
    private int def;
    private float range;
    private float aDelay;
    private bool isAttacking;
    private int speed;

    private int exp;
    private int money;

    private bool flight;
    private bool invisible;
    private bool visible;

    private int remainSep;

    private void Start()
    {
        aIDestinationSetter.target = Player.Instance.transform;
    }

    public void SetSpecialAbility(bool inv, bool fly)
    {
        invisible = inv;
        visible = !inv;
        flight = fly;
    }

    public void SetVisible(bool b)
    {
        visible = b;
    }

    public virtual void SetData(Enemy data, int remainSep = -1)
    {
        this.data = data;

        visible = !invisible;
        // remainSep�� �����ϴ� ���� �ƴ� ���� ����.
        this.remainSep = (remainSep != -1) ? remainSep : data.separate;
        // �и��� ����ŭ ü���� divide
        maxhp = hp = (int)(data.hp / Mathf.Pow(2, data.separate - this.remainSep));
        hpBar.SetHpBar(maxhp, new Vector2(spriteRenderer.sprite.rect.width / spriteRenderer.sprite.pixelsPerUnit, spriteRenderer.sprite.rect.height / spriteRenderer.sprite.pixelsPerUnit));

        aiPath.maxSpeed = speed = data.speed;
        dmg = data.dmg;
        range = data.range;
        aDelay = data.adelay;

        exp = data.exp;
        money = data.money;

        WaitAttack = false;
        activatedBuff = null;
    }

    private Color currentColor;
    private static Color VisibleColor = new Color(1, 1, 1, 1);
    private static Color InvisibleColor = new Color(1, 1, 1, 0);
    private static Color ScanedInvisibleColor = new Color(1, 1, 1, .5f);

    public override void Update()
    {
        base.Update();

        // ������ �ʴ� ���·� ����.
        Color color = InvisibleColor;
        if (visible)
        {
            // ������ �ʴ� ���������� ���̴� ������ ��
            if (invisible) color = ScanedInvisibleColor;
            // �׳� ���̴� ������ ��
            else if (!invisible) color = VisibleColor;
        }
        SetColor(color);

        if (GameController.Instance.Pause) aiPath.canMove = false;
    }

    private void SetColor(Color color)
    {
        if (currentColor == color) return;
        currentColor = color;
        spriteRenderer.color = color;
        hpBar.gameObject.SetActive(visible);
    }

    #region IDamagedObject
    public int Hp { get { return hp; } }
    public int Def { get { return def + (activatedBuff != null ? activatedBuff.def : 0); } }

    public void Heal(int amount)
    {
        hp += amount;
        if (hp > maxhp) hp = maxhp;
        hpBar.UpdateHpBar(hp);
    }

    public void Damaged(int dmg)
    {
        // ���º��� �������� ���ٸ� �������� 1�� ����.
        dmg -= Def;
        if (dmg < 0) dmg = 1;

        hp -= dmg;
        hpBar.UpdateHpBar(hp);
        StartCoroutine(ChangeColor());
        if (hp <= 0)
        {
            if (remainSep > 0)
            {
                remainSep--;
                SetData(data, remainSep);
                string prefabName = EnemyController.GetEnemyPrefabName(data);

                EnemyObject sepObject = (EnemyObject)PoolController.Pop(prefabName);
                sepObject.SetData(data, remainSep);
                EnemyController.Instance.AddEnemy(sepObject, transform.position);
            }
            else
            {
                int rand = Random.Range(0, 2);
                if (rand == 0) Item.Drop(transform.position);

                Player.Instance.GetReward(exp, money);
                Dead();
            }
        }
    }

    public void Dead()
    {
        PoolController.Push(gameObject.name, this);
        //spriteRenderer.color = Color.green;
        StopAllCoroutines();
        EnemyController.Instance.DeadEnemy(this);
    }

    IEnumerator ChangeColor()
    {
        //spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        //spriteRenderer.color = Color.green;
    }
    #endregion

    #region IAttackObject
    public Collider2D TargetCollider { get { return targetCollider; } }
    public int Dmg { get { return dmg + (activatedBuff != null ? activatedBuff.dmg : 0); } }
    public float Range { get { return range; } }
    public float ADelay { get { return aDelay / (activatedBuff != null ? activatedBuff.aspeed : 1); } }
    public bool WaitAttack { get; set; }

    public bool DetectTarget()
    {
        // ���� ������ �������� �� Failure�� ����� ��. (���� ��� ���̳� �÷��̾�)
        // �� �ܿ� ������ �پ����� ���� �̵��ϴ� ��ĵ� ����� �ʿ䰡 ����.
        // ���� ��� ������ �پ��ְ� ���� ���� ���� ���̶�� �̵��� �� �ʿ䰡 ����.
        // �ٸ� �� ���� �������ݿ� ����. ���Ÿ����� �����̶�� �տ� �ٰŸ� ������ ���� ���� �����Ƿ�
        // ������ ������ ���� ������ �ؾ��� ���� ����.
        // �̷� ��� ��Ȳ�� ���� ó���� �ʿ���.

        // ���� ���� ������ ���� Player�� �ֺ��� �ִ��� üũ
        // �ִٸ� �ش� ������ Ÿ������ ����
        // ���ٸ� Turret�̶� �ִ��� üũ
        // �ִٸ� ���� ù �ͷ��� Ÿ������ ����.

        // �������̶�� �ش� Ÿ���� ������ ���� ������ üũ
        // �װ� �ƴ϶�� 0.75������ �ȿ� �ֳ� üũ

        float range = this.range;
        if (!isAttacking) range *= .75f;

        targetCollider = Physics2D.OverlapCircle(transform.position, range, 1 << LayerMask.NameToLayer("Player"));
        if (targetCollider == null)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, range, 1 << LayerMask.NameToLayer("Turret"));
            if (cols != null && cols.Length > 0) targetCollider = cols[0];
        }

        if (targetCollider == null) isAttacking = false;
        else
        {
            LookAt(targetCollider.transform.position);
            aiPath.canMove = false;
        }

        return targetCollider != null;
    }

    public void Attack()
    {
        isAttacking = true;
        if (!WaitAttack)
        {
            IDamagedObject damagedObject = targetCollider.transform.parent.GetComponent<IDamagedObject>();
            damagedObject.Damaged(Dmg);
            StartCoroutine(AttackTimer());
        }
    }

    private void LookAt(Vector3 target)
    {
        Vector2 dir = target - transform.position;
        float degree = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);
        transform.rotation = Quaternion.Euler(0, 0, degree - 90);
    }

    public IEnumerator AttackTimer()
    {
        WaitAttack = true;
        float time = 0;
        while (time < ADelay)
        {
            if (!GameController.Instance.Pause) time += Time.deltaTime;
            yield return null;
        }
        WaitAttack = false;
    }
    #endregion

    #region IMovingObject
    public int Speed { get { return speed; } }
    private Vector3 direction;
    private float moveAmount;
    private bool isMove;
    public bool DetectPath()
    {
        // �켱 path�� ������ ã�´ٰ� ����.
        return true;
    }

    public void Move()
    {
        direction = direction.normalized;
        rigidbody.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aiPath.canMove = true;
    }

    #endregion

    #region IBuffTargetObject

    public BuffInfo ActivatedBuff { get { return activatedBuff; } }
    private BuffInfo activatedBuff;

    public bool BuffIsActivated { get { return activatedBuff != null; } }

    public void ActivateBuff(BuffInfo buff)
    {
        // �ܼ� ȸ���� ��� ��� �ߵ�
        if (buff.IsHeal) Heal(buff.hp);
        else
        {
            activatedBuff = buff;
            StartCoroutine(BuffTimer());
        }
    }

    public IEnumerator BuffTimer()
    {
        float time = 0;
        while (time < activatedBuff.time)
        {
            if (!GameController.Instance.Pause) time += Time.deltaTime;
            yield return null;
        }
        activatedBuff = null;
    }
    #endregion
}
