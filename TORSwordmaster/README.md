# Swordmaster Career

## Introduction
The Eonir always lacked a real melee career. I made Council Guard in an attempt to create one, but it always felt like it lacked the special secret sauce, a more complex and engaging set of mechanics, and perhaps a bit higher power ceiling. 

So, coming from my love of Warrior Priest classes and Warrior Priest of Thungni (Runesmith), I based a new class on a single 4e source and created the Swordmaster Class, based on the idea that traditions similar to, but not an exact imitation of Swordmaster of Hoeth survives in reclusive elven colony (aka the Eonir). 

It should be pretty decently loreful with a bit of lore stretch. Not released yet as I am tuning numbers and trying to replace some contents. 

*Trained in the ancient art of Sword-Dancing, you perform a form of High Magic with an enchanted sword, passed down as a family heirloom in your family. Your forms are similar, and an older form of the same martial art that are taught to the famous Swordmasters of Hoeth in Ulthuan.*

**Premise:** Melee focused career. Pseudo caster akin to a warrior priest, without sustain. Powerful personal fantasy and power. A follow on from Council Guard.

**Structure:** 1 - 3 - 3 to emphasize mid late power. Runelord uses the same shape.

**Career Ability:** Way of the Sword

Enter a trance in which you and your blade move as one. Gain +15% Physical Melee Damage, +15% Swing Speed, +10% Movement Speed on foot and your Techniques, which spend charge. Charge drains 2.5% per second; the trance ends when it runs out. Gain charge from melee damage (2% to 10% per hit, up to 12% per swing) and blocks (8%, shield blocks 5%, once per second), perfect parries (15%), chambers and missile deflections (30%). Requires a melee weapon. Each point in your highest melee skill adds 0.03% Physical Melee Damage.

- Requires full charge to enter. Entering refills to 100%
- The ability lasts until charge runs out, or you use Way of the Sword again at full charge.

| Charge source | Charge | Limit |
|---|---|---|
| Melee damage | 1% per 10 damage, min 2%, max 10% per hit | 12% per swing (hits within 0.3s) |
| Held block | 8% | Once per 1s |
| Shield block | 5% | Once per 1s (shared with held blocks) |
| Perfect parry | 15% | - |
| Projectile deflection | 30% | - |
| Chamber | 30% | - |
| Friendly hits | 0% | - |

**Career Button:** Martial Training

Train a 'Melee' unit for 125 Favor, giving them a small but significant permanent buff. Following training paths available:

- Path of the Rain: 25% Physical Ranged Resistance, 10% Movement Speed
- Path of the Hawk: 10% Swing Speed
- Path of Frost: 15% Physical Damage

Uses the Knight of the Old World seal pattern (permanent troop attributes through the career button helper).

### Swordmaster (Clan Tier 1)

**Sword-Dancing:**

- +20 personal Hitpoints
- +10% personal swing speed.
- +15% personal 'Physical' melee damage.
- Gain 'Faith' skill per melee kill, scaled by the victim's level.
- Way of the Sword grants 'Cleave'.

### Blademaster (Clan Tier 2)

**Heirloom of Aenarion:**

- +20% personal 'Armour Penetration' for melee attacks.
- Flight of the Phoenix also dismounts riders it hits.
- 2% physical resistance, +5 personal hit points for each enchanted item on you.
- +10% personal 'Physical' melee damage.
- Ability damage +0.03% per 'Leadership'. Flight of the Phoenix makes everything it hits bleed for 5 damage per second for 6s.

**Thirty Forms:**

- +20 One-Handed / Two-Handed skill for all troops
- +25 daily experience for all 'Melee' troops
- -33% Resource cost for 'Melee' troops upgrades
- -20% wages for all 'Melee' troops.
- Ability swing speed scales with 'Faith' (+0.03%/pt) and applies to allies within 5m.

**Path of the Storm:**

- After a block or parry, gains +50% damage on your next strike.
- +20% personal physical ranged resistance while not using a shield
- Hits below 20 damage no longer stagger you.
- Perfect Parry, Chamber Block and Deflection grant 50% more charge.
- Way of the Sword: +50% ranged 'Physical' resistance. Each missile that hits you costs 3% charge.

### Bladelord (Clan Tier 4)

**Sword of Hoeth:**

- +5 Companion Limit.
- +10% 'Physical Resistance' for 'Melee' troops
- +15% 'Physical' melee damage and +10% Ward Save for Swordmaster companions.
- Swordmaster companions gain +100 daily Two-Handed and Faith experience.
- Path of the Sun from you or a companion also grants 'Unstoppable' and +10% Ward Save.

**Bladelord:**

- +30 personal Hitpoints.
- +10% personal armour penetration for melee attacks.
- +15% Physical damage for 'Melee' troops.
- Path of Falling Water deals +0.2% damage per point in your highest melee skill.
- Unlocks the Master's Strike technique.

**Ritual of Cleansing:**

- Entering Way of the Sword removes harmful effects from you, once per 30s.
- +20% personal 'Magical' resistance.
- Begin battles with 50% Way of the Sword charge.
- +10% personal physical resistance while Way of the Sword is active.
- -20% technique cooldowns. Shadows of Loec also cleanses you.

## Charge & Techniques

Way of the Sword will be the magic "school" name, five techniques. They are Prayer type (Faith experience, no Winds), with independent cooldowns. Each costs Way of the Sword charge, and can only be used while Way of the Sword is active.

| Technique | Charge | Cooldown |
|---|---|---|
| Way of the Sword | Requires 100%, refills on entry | - |
| Flight of the Phoenix | 10% | 15s |
| Shadows of Loec | 10% | 15s |
| Path of the Sun | 40% | 30s |
| Path of Falling Water | 50% | 20s |
| Master's Strike | 50% | 30s |

**Flight of the Phoenix (Base):**

- Short-range projectile, stops on the first enemy, ignoring armor, dealing physical damage. Shield piercing.
- 50 damage to the first enemy, then 35 damage in a 6m line behind them.
- Damage +0.2% per point in your highest melee skill.

**Shadows of Loec (Novice):**

- Knocks down foes within 5m and gains +75% Movement Speed for 6s, only works on foot.

**Path of the Sun (Adept):**

- Whips allies within 10m into a frenzy with your sword, granting them +20% Physical Damage and +10% Physical Resistance. 20s duration.

**Path of Falling Water (Grand):**

- AOE attack. 60 Physical damage in 4m radius, only works on foot.

**Master's Strike (Bladelord keystone):**

- Your next two strikes deal +100% 'Magical' damage, are unparryable and have 100% armor penetration.

## Swordmaster Companions
- Currently only recruitable by a Swordmaster (Might expand to Eonir, in general, as it is an interesting companion / class type fulfilling a pseudo priest role)
- Cost 100k and 1000 Favor
- Starts in Rusty version of Queen's Champion gear
- Skills: 140 Two-Handed, Athletics 110, One-Handed 60, Faith 60.
- Companion version of Swordmaster abilities with a shared cooldown.
- Companion Flight of the Phoenix has no bleed or dismount. Companion Path of Falling Water does not get the Bladelord scaling and is not foot-locked.

| Technique | Companion cooldown |
|---|---|
| Flight of the Phoenix | 15s |
| Shadows of Loec | 15s |
| Path of the Sun | 30s |
| Path of Falling Water | 20s |

## Hireling Activties
- Practice the Way of the Sword (Two-Handed)
- Perform the sword-dance until exhausted (Athletics)
- Meditate to balance your Yenlui (Faith)
- Discuss tactics with the army commanders (Tactics)
- Drill the soldiers in swordmanship (Leadership)