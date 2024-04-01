using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] protected Pathfinding.Seeker seeker;

    public Enemy Data { get { return data; } }
    protected Enemy data;

    protected int hp;
    protected int maxhp;

    protected int dmg;
    protected int def;
    protected float range;
    protected float aDelay;
    protected int speed;

    protected int exp;
    protected int money;

    protected bool flight;
    protected bool invisible;
    protected bool visible;

    protected int remainSep;

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
        maxhp = hp = (int)(data.hp * GameController.Instance.Difficulty.hp / Mathf.Pow(2, data.separate - this.remainSep));
        Vector2 spriteSize = new Vector2(spriteRenderer.sprite.rect.width, spriteRenderer.sprite.rect.height) / spriteRenderer.sprite.pixelsPerUnit;
        hpBar.SetHpBar(this, maxhp, new Vector2(spriteSize.x * 3 / 2, 0.25f), spriteSize.y * 3 / 4);

        speed = data.speed;
        dmg = data.dmg;
        range = data.range;
        aDelay = data.adelay;

        exp = data.exp;
        money = data.money;

        WaitAttack = false;

        moveTarget = Player.Instance.transform;
        moveAmount = 0;
        StartCoroutine(DetectingPath());
    }

    private Color currentColor;
    private static Color VisibleColor = new Color(1, 1, 1, 1);
    private static Color InvisibleColor = new Color(1, 1, 1, 0);
    private static Color ScanedInvisibleColor = new Color(1, 1, 1, .5f);

    public override void Update()
    {
        base.Update();
        moveAmount += Time.deltaTime;
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
    public int Def { get { return def + ActivatedBuff.def; } }

    public void Heal(int amount)
    {
        hp += amount;
        if (hp > maxhp) hp = maxhp;
    }

    public void Damaged(int dmg)
    {
        // ���º��� �������� ���ٸ� �������� 1�� ����.
        dmg -= Def;
        if (dmg < 0) dmg = 1;

        hp -= dmg;
        if (gameObject.activeSelf) StartCoroutine(ChangeColor());
        if (hp <= 0)
        {
            if (remainSep > 0)
            {
                remainSep--;
                SetData(data, remainSep);

                Vector3 sepAmount = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z) * Vector2.right / 2;

                EnemyObject sepObject = EnemyController.Instance.AddEnemy(data, transform.position + sepAmount);
                transform.position -= sepAmount;

                sepObject.SetData(data, remainSep);
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

    public virtual void Dead()
    {
        PoolController.Push(gameObject.name, this);
        //spriteRenderer.color = Color.green;
        buffs.Clear();
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

    protected bool isAttacking;
    protected Collider2D targetCollider;
    public Collider2D TargetCollider { get { return targetCollider; } }
    public int Dmg { get { return (int)((dmg + ActivatedBuff.dmg) * GameController.Instance.Difficulty.dmg); } }
    public float Range { get { return range; } }
    public float ADelay { get { return aDelay * (1 + ActivatedBuff.aspeed); } }
    public bool WaitAttack { get; set; }

    public virtual bool DetectTarget()
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

        float range = Range;
        FindTargets(range, 0.75f, 1 << LayerMask.NameToLayer("Player"));
        if (targetCollider == null)
        {
            int layerMask = 1 << LayerMask.NameToLayer("Turret");
            // ���Ÿ� ������ ��� Trap�� ������ �� ����.
            if (Range >= 3f) layerMask |= 1 << LayerMask.NameToLayer("Trap");
            FindTargets(range, 0.75f, layerMask);
        }

        if (targetCollider == null) isAttacking = false;
        else LookAt(targetCollider.transform.position);

        return targetCollider != null;
    }

    protected Collider2D[] FindTargets(float range, float detectRatio, int layerMask)
    {
        if (!isAttacking) range *= detectRatio;
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, range, layerMask);
        if (cols.Length > 0) targetCollider = cols[0];

        return cols;
    }

    public virtual void Attack()
    {
        isAttacking = true;
        if (!WaitAttack)
        {
            IDamagedObject damagedObject = targetCollider.transform.parent.GetComponent<IDamagedObject>();
            damagedObject.Damaged(Dmg);
            StartCoroutine(AttackTimer());
        }
    }

    public void LookAt(Vector3 target)
    {
        Vector2 dir = target - transform.position;
        float degree = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);
        transform.rotation = Quaternion.Euler(0, 0, degree - 90);
    }

    protected IEnumerator AttackTimer()
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
    public float Speed { get { return speed * (1 + ActivatedBuff.speed) * GameController.Instance.Difficulty.speed; } }
    protected Transform moveTarget;
    protected List<Pathfinding.GraphNode> path;
    private int pathIndex;

    private IEnumerator DetectingPath()
    {
        while (true)
        {
            float time = 0;
            while (time < 1f)
            {
                if (!GameController.Instance.Pause) time += Time.deltaTime;
                yield return null;
            }
            SetPath();
        }
    }

    protected void SetPath()
    {
        path = seeker.StartPath(rigidbody.position, moveTarget.position).path;
        pathIndex = 1;
    }

    public virtual bool DetectPath()
    {
        // ��Ž���� �ƴ���, �� �� �� �ִ� ��尡 �ִ��� Ȯ��.
        return seeker.IsDone() && path != null && path.Count - 1 > pathIndex;
    }

    protected float moveAmount;
    public void Move()
    {
        var next = IMovingObject.GetPos(path[pathIndex].position);
        var dir = (next - rigidbody.position).normalized;
        LookAt(next);
        // Update���� ȣ��Ǳ� ������ deltaTime�̿�.
        rigidbody.position += dir * Speed * moveAmount;
        if (IMovingObject.EndOfPath(rigidbody.position, next, dir * Speed * moveAmount)) pathIndex++;
        moveAmount = 0;
    }
    #endregion

    #region IBuffTargetObject

    public List<BuffInfo> Buffs { get { return buffs.Keys.ToList(); } }
    private Dictionary<BuffInfo, IEnumerator> buffs = new Dictionary<BuffInfo, IEnumerator>();
    public BuffInfo ActivatedBuff
    {
        get
        {
            BuffInfo buff = new BuffInfo();
            if (Buffs != null) Buffs.ForEach(b => buff += b);
            return buff;
        }
    }

    public void ActivateBuff(BuffInfo buff)
    {
        // �ܼ� ȸ���� ��� ��� �ߵ�
        if (buff.IsHeal) Heal(buff.hp);
        else
        {
            if (buffs.ContainsKey(buff))
            {
                StopCoroutine(buffs[buff]);
                buffs[buff] = BuffTimer(buff);
            }
            else buffs.Add(buff, BuffTimer(buff));

            StartCoroutine(buffs[buff]);
        }
    }

    public IEnumerator BuffTimer(BuffInfo buff)
    {
        float time = 0;
        while (time < buff.time)
        {
            if (!GameController.Instance.Pause) time += Time.deltaTime;
            yield return null;
        }
        buffs.Remove(buff);
    }
    #endregion
}
