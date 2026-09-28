# The Old Realms - Mercenary Overhaul Submod
Repository for the Mercenary Overhaul submod, which I made for me and my friends personal use to make the career better and more fun and thematic to play, instead of feeling like a placeholder. 

## Content
Overhauls the Mercenary Career to add and change some unique mechanics, inspired heavily by Dogs of War and the thematic. Mercenary stays as a career with few personal power on the battlefield, good active buffs and unique campaign mechanics. My goal is to make it feels like less of a placeholder career but an actual career for Empire, Eonir and Dawi.

This career overhaul is not fully designed to work for Sylvania, Mousillon or Bretonnian mercenaries. I think these three factions are far too tied to Undead and Knights to work properly with how this overhaul does things.

## Changes

### Career Ability 
Career Ability: Let Them Have It!
Never retreat! Never surrender! Inspire troops in an area. Allies within become 'Unbreakable' and 'Unstoppable' for 30s. Gains 0.05m Radius and 0.05s per point of Leadership.
* Aimed up to 30m away. Base radius of 6m, 60s Cooldown.

### Specialization - Mercenary Contact:
- All but Bretonnian Mercenary choose a company they have a Mercenary contact with. This will guarantee a small amount of them will be hireable in the backstreets (Tavern menu) and also ties into your Career Button (For Empire). This is shown on a separate line from ordinary mercenaries
- Your choices are the 6 non-cursed Company Dogs of War in game: Braganza's Besiegers, Marksmen of Miragliano, Leopold's Leopard Company, Ricco's Republican Guard, Voland's Venators, Al-Muktar's Desert Dogs
- As you tier up, the odds of the guaranteed recruits being higher tier shifts: 55/30/15 at Recruit, 30/40/30 at Sergeant, 15/35/50 at Commander.

### Career Button - Mercenary Recruit
* Click on a Tier 1 / 2 recruit of your own culture to pay a cost in gold and / or faction resources to upgrade them.
* Shift Click = 5x
* Empire - Upgrade into a T4 troop of the company you chose. 750g, 4 Prestige. 
* Eonir - Upgrade into a Sea Elf Guard. 300g. Since you can just pick them up off from tavern anyway.
* Dawi - Upgrade into a Dawi Ranger. Pays 150g. (Cheapest, because Brewers Guild also offers Dawi Rangers at a very low price)

#### Elite Recruitment (Tier 2) 
* Only works for T3 or above units of your own culture.
* T3+ redirects to the Elite route.
* Empire upgrades into Paymaster's Bodyguard for 750g and 10 Prestige, which reuses Griffon Knight's gearset with Empire Greatsword. It is a T7 Greatsword Shock troops that has 25% physical amp and unbreakable, meant as a more easily obtained Ostland Black Guard . Perfect for Greatsword LARP. Based on the tabletop unit. 
* Dawi upgrades into Bugman's Ranger for 750g and 10 Oathgold. 
* Eonir upgrades into Sea Elf Sentinel for 450g. White Lion of Chrace will stay as an exclusive through the faction mechanics
* Unit at or above the Tier is refused 

#### Cross Cultural Recruitment (Tier 4)
* You gain access to the non-elite upgrade button of aligned culture
* Empire can upgrade Dawi and Eonir recruits into Ranger / Sea Elf Guard (For Empire, this is diverse, but also largely a convenience things as you can pick it up in tavern)
* Dawi can upgrade Empire recruits into their contact company, using their resources (I.e. Recruit into Voland)
* Eonir can upgrade Empire recruits into their contact company, using their resources (I.e. Recruit into Pikemen)
* Bretonnia, Mousillon and Sylvanian Mercs do not get the button

### Career Perks
Revamped some of the Career perks. In addition, I also made it so that your Career Ability can actually be cast remotely - as I realized as a Mercenary I sometimes want to buff my ranged line instead of my melee line and I think that clears some design tensions since you can select to buff your melee or your ranged as needed. 

Recruit (clan level 1)
* The Survivalist:
   * +3 extra ammo per ammunition pouch, or quiver you equip.
   * +5% personal ranged 'Physical' damage.
   * +1 party move speed on campaign map.
      - Replaces TOR's +20% in forest, mountain or swamp terrain.
   * Personal healing rate increased by +1.
      * Swapped from Duelist for thematic
      - TOR's once-a-day hunt that used to sit here is switched off.
   * Let Them Have It! also provides +15% 'Physical Resistance' and begins the battle ready.

* The Duelist:
   * +10 personal Hitpoints.
   * +5% 'Physical Resistance' for melee troops.
   * +5% personal melee 'Physical' damage.
   * Kills in combat grants 'Leadership' experience.
      * Design Note: Leadership is hard to level up early mid game, having access to the primary scaling stats leveling up w/o needing to rely on Hireling is good and set up a sense of progression. Also encourages you to level up an ability otherwise not too important for the main character.
      - 10 x the victim's level per kill (about 210 XP for a level 21 troop).
   * Let Them Have It! also provides +15% swing speed.


Sergeant (clan level 2)

* The Headhunter:
   * +5 personal ammunition.
   * +5% personal ranged 'Physical' damage.
   * +5 Companion limit.
   * +10% personal ranged 'Physical Resistance'
   * Let Them Have It! also provides +15% 'Physical' ranged damage.
* The Knightly:
   * +10% personal melee 'Physical' damage.
   * +10% personal melee 'Physical Resistance'.
   * +20 personal Hitpoints.
   * +8% personal 'Armour Penetration' of melee attacks.
   * Let Them Have It! also provides +15% melee 'Physical' damage.
* The Paymaster:
   * Wounded troops heal faster.
   * +40% chance to recruit 2 troops instead of 1.
   * -10% Gold upkeep for tier 4+ troops.
   * Mercenaries in taverns appear in greater numbers and cost 25% less.
      * Swapped from the Companion upgrade button, which I never liked and you are better off long term fielding "real" companions.
      - 50% more tavern mercenaries. The town's tavern mercenary and your contact offers are both 25% cheaper.
   * The effects of Let Them Have It! are doubled.


Commander (clan level 4)

* The Mercenary Lord:
   * +3 extra ammo per pouch of Grenades or Buckshot.
   * +10% 'Physical' damage for ranged troops.
   * Mercenary contracts 'Influence' cost, and payout scale with your Trade skill.
   * +100% 'Faction Resource' from battles. Gain another 100% if Leadership reaches 300.
      - No actual changes. TOR already applied it to every resources but the card wasn't updated.
   * Each of your kills shortens Let Them Have It!'s cooldown by 3s. It also provides +15% reload speed.
* The Commander:
   * +5 Companion limit.
   * +10% 'Physical' damage of 'Melee' troops.
   * Hits below 15 damage no longer stagger you.
   * +15 Hitpoints for Companions.
   * The base radius of Let Them Have It! is doubled.
      - Doubles the 6m base only. Leadership is added on top: 12m + 0.05m per point.

## Design Notes & Such 
Bullet Points! 
- Mercenary Career should encourages you to invest in being a good leader / quartermaster, since that's part of the fantasy and DOW and legendary Mercenary leader emphasizes leadership and not personal prowess
- Keeps the very modest scaling of personal power in vanilla Mercenary. It is all about actively buffing your troops.
- Passive buffs are modest, but universal, same as vanilla, I don't want to lock Merc dead into fielding a particular type of troops, that is for specialized careers. 
- Campaign mechanics let you replenish and fields unique units which should add some options. 
- Mercenary Contact lets you field the 6 Dog of War companies as a player, I think 5 of them will find use with an Empire player, and Voland's Venators would be potentially useful for Dawi for versatilities
- Eonir I make it so that fielding Sea Elf is even easier and require less trips to taverns, as Sea Elf are versatile infantry-skirmishers and are mercenaries
- For Dawi I used Rangers, which may or may not be that great (Bugman's Ranger is great, however) and is the one vanilla unit that is a free slot
- I really liked Warrior Priest of Sigmar and any careers that incentivize you to engage in melee combat which I feel is the point of playing Bannerlord. Therefore, the Mercenary Lord capstone is designed to encourage you to do exactly that. And it is why I also make sure you get Leadership XP very early on for killing and made sure your ability scales to that. There's no multiplicative aggressive stacking - you just slap your flat but powerful and no WOM buffs in an increased radius as you get better.  
- The improved and buffed Let Them Have It should be a cool buff without going too deep into Not Magic territory.
- I left most of the conservative scaling and abilities intact. +10 Companion is the strongest companion bonus and very powerful in TOR and I left it completely alone.

## Future Design Notes / Things 
- Needs to make Human counts for -25% party size for Dawi Merc, otherwise there isn't much of a point to field them with how constrained their party size and how good their units are. 
- If Bretonnian ROR becomes worth fielding, I may or may not add a Merc career, but I have very low interest because I think Bretonnia is best experienced as a Knight or Damsel. It shouldn't takes too much efforts to adjust this career to work for them.

## License 
GPL-3.0 since it needs TOR_Core

## Versions
Will be maintained on NexusMods, to avoid bricking people's saves whenever I make any save incompatible update.

## Compatibility 
Compatible with TOR, not guaranteed to work with any other mods. May or may not support you on Discord. May or may not maintain or further work on this mod. This is made for myself and my friends and released for the world to enjoy. PRs not accepted - read only.

## Credits
Credits to The Old Realms team for the original mods. I do not make any profit nor receive any money from this work. 