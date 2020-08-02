using UnityEngine;
using System.Collections;
public class DamageSystem
{

    private double hp;
    private double maxHp;
    private double damg;

    public DamageSystem(double maxHp, double damg)
    {
        this.maxHp = maxHp;
        this.damg = damg;
        hp = maxHp;
    }

    public double getHp()
    {
        return hp;
    }

    public double getDam()
    {
        return damg;
    }

    public void setHp(double newhp)
    {
        maxHp= newhp;
    }

    public void setDam(double newdam)
    {
        damg = newdam;
    }

    public void damage(double dam)
    {
        hp -= dam;
        if (hp < 0.0) hp = 0.0;
    }

    public void heal(double heal)
    {
        hp += heal;
        if (hp >= maxHp) hp = maxHp;
    }
}
