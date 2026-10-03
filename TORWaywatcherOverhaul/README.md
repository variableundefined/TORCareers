# Waywatcher Overhaul
An overhaul of the TOR Vanilla Waywatcher Career, based on current lore on how Asrai longbow are equipped with six types of enchanted arrows. The goal is to make you feels like a magical archer from Level 1, improves the gameplay varieties from Level 1 to 30 / 35. 

It does this by adding in six abilities and arrows you can select from the ability wheel, and redo how Lethal Shot and its keystones scales: 

**Career Ability:** Lethal Shot

Soon, the Lumberfoots shall regret their trespass! Fire a Lethal Shot, with deadly precision! Your next 3 arrows fire double the 'Piercing' arrows, and 'Bursting' arrows gain +1m radius. Every 'Keystone' career perk unlocked increases arrows affected by Lethal Shot by +1, but decreases its recharge rate. (Ability is charged by dealing damage with bows.)

- Arrows: 3, +1 per Keystone, +1 more from Protector of the Woods.
- Charge: -10% per Keystone unlocked.
- Duration: 20s, or until the Lethal Shot arrows are spent.

**Career Feature:** Enchanted Arrows

- Switching is instant and applies to every arrow you shoot.
- Ability Icons: Drawn by yours truly in Aseprite.

## Enchanted Arrows
Enchanted Arrows is the main career feature of the overhauled Waywatcher and let you select between 6 arrow types mid combat with different utilities.

T2 / T3 show the arrow's full effects once you pick the Keystone meant to unlock Enchanted Arrows of that tier. 

T2 Enchanted Arrows required Hail of Arrows keystone, while T3 needs Eye of the Hunter keystone.

Arrows are classified into 'Piercing' or 'Bursting' arrows based on their interaction with Lethal Shot. I originally considered 'Lineal' and 'AOE' or 'Single-Target' and 'AOE' but I can't find a satisfying term.

| Arrow Type | T1 | T2 | T3 | Lethal Shot |
|---|---|---|---|---|
| **'Piercing' Arrow** | | | | |
| Swiftshiver Shards | 2 shards, 60% damage each, narrow cone | 4 shards, 45% damage each | 6 shards, 40% damage each | 2x shards, wider cone. No pierces |
| Trueflight Arrow | 100% damage, +25% missile speed, pierces shields | 125% damage, +50% missile speed | 150% damage, +100% missile speed | 2x stacked arrows. From T2 Onward: pierce targets |
| Arcane Bodkin | 100% damage, ignores 50% armour, stopped by shields | 125% damage, ignores 75% armour | 150% damage, ignores 75% armour | 2x stacked arrows. From T2 Onward: pierce targets |
| **'Bursting' Arrow** | | | | |
| Starfire Shaft | 110% 'Fire' damage, burns the target (4/s, 6s) | 120% 'Fire', burns 8s | 130% 'Fire', burns 10s, Fire vulnerability | Bursts in 1m (1.5m at T2, 2m at T3), spreading the arrow's full upgraded effect |
| Moonfire Shot | 125% 'Magic' damage, 0.5m shockwave (15 'Magic'), knockdown | 140% 'Magic' | 150% 'Magic', 1m shockwave, Magic vulnerability | +1m shockwave (+1.5m at T2, +2m at T3) |
| Hagbane Tips | 100% damage, poisons the target (4/s, 6s), -30% movement | 5/s for 7s, -40% movement | 5/s for 8s, -50% movement | Bursts in 1m (1.5m at T2, 2m at T3), spreading the arrow's full upgraded effect |

- Outside Lethal Shot, Starfire Shaft and Hagbane Tips only affect the target hit when it is not using Lethal Shot. They are meant to be situational tool, but can be useful vs certain targets like ethereal or trolls.
- Bursts hit friend and foe, except you.
- Fire / Magic vulnerability: Gives -34% 'Fire' / 'Magic' resistance.

## Keystones
No passives were changed from TOR. I only modified the Keystones effect.

| Tier | Keystone | Text |
|---|---|---|
| 1 | Protector of the Woods | Lethal Shot can be used on battle start, reduces required damage to use, and affects +1 shot. |
| 1 | Pathfinder | Melee attacks also charge Lethal Shot. |
| 1 | Forest Stalker | Troop damage also charges Lethal Shot. |
| 2 (Clan Tier 2) | Hail of Arrows | T2 Enchanted Arrows. Lethal Shot: +0.5m 'Bursting' radius, Trueflight and Bodkin pierce. |
| 2 (Clan Tier 2) | Starfire Essence | 'Bursting' arrows deal 25% more area damage. |
| 2 (Clan Tier 2) | Hawkeyed | Shards: 2x Lethal Shot charge. Trueflight: +50% at range. Bodkin: +50% vs armour. |
| 3 (Clan Tier 4) | Eye of the Hunter | T3 Enchanted Arrows. Lethal Shot: +0.5m 'Bursting' radius. |

## Power Curve / Design 
At Tier 1, I expect Waywatcher to feel significantly stronger and better with something to do, since they can access Swiftshiver Shards for headshot from a good angle and Trueflight / Arcane Bodkin for their staple arrows. 

At Tier 2, I expect power to be SLIGHTLY below the existing Waywatcher, which unlocks shield and entity piercing arrow that also spread out at the same time. I did my best to make sure Swiftshiver (Or any of the Lineal) arrows don't become the meta dominating choice, though at the moment the balance is in flux. 

At Tier 3, I expect the power to match or exceed Waywatcher without overhaul slightly. 

Most importantly, I expect you to have fun and not be doing only one thing in combat. 

## Known Quirks
The arrows display as (Prayer). I will not fix / tweak that for now.

## Other Design Notes
I may or may not rework / add additional unit / companion buff to make the class significantly more interesting, but I don't have new inspiration, yet! 

I think Waywatcher should stay a very focused on "me myself and I" with passive unit buffs due to Waywatcher's reputation as extremely deadly and almost magical archer in canon.
