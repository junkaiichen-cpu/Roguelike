# Bible Survivors — Project Vision

## 1. Vision

Bible Survivors is a Survivors-like action roguelite built around
Biblical characters, Biblical history, narrative progression,
scripture learning, and long-term character mastery.

The game combines two experiences:

1. A highly replayable action game.
2. A coherent journey through Biblical stories.

The player should come back because:

- the gameplay is fun
- builds are satisfying
- characters are interesting
- progression is meaningful
- new stories are unlocked
- character eras are unlocked
- achievements provide goals
- Biblical knowledge is reinforced
- every run can contribute to long-term progression

---

# 2. Design Pillars

## Pillar 1 — Fun First

The game must be enjoyable even before the narrative layer is considered.

Core satisfaction:

Move
→ attack
→ destroy enemies
→ collect XP
→ level up
→ choose upgrades
→ become stronger
→ survive escalation
→ defeat boss

---

## Pillar 2 — Bible Is the World

Bible content should not be a cosmetic theme.

Characters, environments, events, progression,
encounters and narrative should be rooted in Biblical history.

---

## Pillar 3 — Story Must Have Continuity

The player's experience with a character should form a coherent timeline.

Random gameplay should never randomly destroy narrative chronology.

---

## Pillar 4 — Learn Through Story

The player should encounter Biblical events through storytelling.

The intended flow is:

Gameplay
→ Encounter
→ Story
→ Scripture
→ Reflection / Quiz
→ Achievement
→ Progression

---

## Pillar 5 — Long-Term Character Mastery

Characters are not one-time unlocks.

A character can represent different stages of their life.

Example:

David

Young David
→ Warrior David
→ King David

The player gradually discovers the character's journey.

---

# 3. Target Gameplay Loop

## Moment-to-Moment

Move
→ automatically attack
→ dodge
→ collect XP
→ choose upgrade
→ repeat

## Run Loop

Start Run
→ survive waves
→ build character
→ discover weapons
→ evolve/fuse build
→ defeat elites
→ defeat boss
→ Victory / Defeat

## Meta Loop

Run
→ Currency / rewards
→ permanent upgrades
→ unlocks
→ stronger future runs

## Narrative Loop

Run milestone
→ Encounter opportunity
→ Story
→ Scripture references
→ Quiz
→ Achievement
→ Character progression

## Long-Term Loop

Character
→ Story chapters
→ Encounters
→ Achievements
→ New character era
→ New gameplay possibilities
→ New stories

---

# 4. Core Gameplay Systems

Eventually the game should contain:

- player movement
- automatic attacks
- weapons
- projectiles
- enemies
- enemy AI
- spawning
- XP
- leveling
- upgrade selection
- weapon evolution/fusion
- stages
- waves
- elite enemies
- bosses
- victory
- defeat
- run rewards
- currency
- permanent upgrades
- shop
- achievements
- character unlocks

---

# 5. Character System

Characters should be represented as data.

A character may contain:

- identity
- display name
- biography
- era
- base stats
- starting weapon
- passive
- story arc
- achievement track
- unlock requirements

Example:

David

Base Character
    ↓
Young David
    ↓
Warrior David
    ↓
King David

Different eras may change:

- stats
- weapons
- passive abilities
- visual presentation
- available story chapters

---

# 6. Story System

Each major Biblical character has a Story Arc.

Example:

## David

Chapter 1
Shepherd

Chapter 2
Anointed by Samuel

Chapter 3
Goliath

Chapter 4
Saul

Chapter 5
Wilderness / Fugitive

Chapter 6
King

Chapter 7
Jerusalem

The exact chapters must eventually be validated against the Biblical
source material.

The game should present these as a coherent progression rather than
random independent facts.

---

# 7. Encounter System

An Encounter is a narrative event attached to the character's current
story progression.

Example:

David Chapter 3
→ Valley of Elah
→ Goliath encounter

The player should receive:

1. narrative
2. scripture reference
3. optional audio narration
4. quiz
5. result
6. achievement/progression

---

# 8. Narrative Format

Target duration:

Approximately 5 minutes per major narrated event.

Narrative should feel like a short audio-book chapter.

Narrative should have:

- introduction
- context
- conflict
- development
- climax
- conclusion
- scripture connection

Avoid fragmented encyclopedia-style writing.

---

# 9. Scripture

The intended scripture layer uses:

Chinese New Punctuation Union Version
新标点和合本

The system should represent scripture structurally through references.

Example:

1 Samuel
17
32-50

Scripture text licensing must be reviewed before distribution.

---

# 10. Quiz

Major narrative events may end with five questions.

Difficulty should correlate with:

- story complexity
- gameplay difficulty
- progression stage

Passing the quiz may award:

- achievement
- currency
- story completion
- character progression
- unlocks

---

# 11. Achievement Philosophy

Achievements should provide meaningful goals.

Categories:

Gameplay
Story
Character
Scripture
Quiz
Collection
Mastery

Character achievements should contribute to character-era progression.

---

# 12. Weapon Philosophy

Weapons should feel mechanically satisfying.

The game should eventually support:

Base weapon
→ upgrades
→ combinations
→ evolution/fusion

The system should be generic.

Do not hard-code weapons directly into character code.

---

# 13. Originality

The project takes inspiration from the Survivors-like genre.

The project must NOT become a direct clone of any specific commercial game.

Do not copy:

- source code
- artwork
- music
- sound effects
- exact UI
- exact characters
- exact item names
- exact weapon designs
- exact progression numbers
- exact balance tables
- proprietary assets

The goal is to capture the strengths of the genre:

- accessible controls
- automatic attacks
- satisfying power growth
- large enemy populations
- build experimentation
- meaningful progression
- replayability

while creating original game content and identity.

---

# 14. Technical Vision

Technology:

Godot 4
C#

Architecture:

Data-driven
Modular
Testable
Content-expandable

C# should provide systems.

Resources/data should provide content.

---

# 15. Content Scalability

The architecture must make it inexpensive to add:

Character N
Story N
Encounter N
Weapon N
Enemy N
Stage N

Adding a new character should primarily involve content definitions,
not rewriting the gameplay engine.

---

# 16. MVP

The first playable MVP is intentionally small.

Required:

- player
- movement
- basic enemy
- basic automatic weapon
- damage
- enemy death
- XP
- level
- upgrade choice
- basic wave escalation
- basic boss
- victory
- defeat
- restart

No complex narrative system is required for the first MVP.

---

# 17. Development Order

Phase 0
Project audit and architecture

Phase 1
Core Survivors loop

Phase 2
Weapons and evolution/fusion

Phase 3
Stages, waves and bosses

Phase 4
Meta progression

Phase 5
Shop and achievements

Phase 6
Character system

Phase 7
Bible content model

Phase 8
Story arc system

Phase 9
Encounter system

Phase 10
Scripture system

Phase 11
Quiz system

Phase 12
Character mastery / eras

Phase 13
Audio narrative

Phase 14
Content expansion and polish

---

# 18. Success Criteria

The project succeeds when:

A new player can start a run immediately.

The combat is satisfying.

The player becomes noticeably stronger during a run.

The player has meaningful build decisions.

Runs create permanent progression.

Characters have recognizable identities.

Biblical stories form coherent chronological journeys.

Encounters feel like meaningful events rather than random text boxes.

Scripture is presented responsibly.

Quizzes reinforce story comprehension.

Achievements create long-term goals.

Character eras create meaningful progression.

New Biblical characters can be added primarily through content data.

---

# 19. Product Philosophy

The player should gradually feel:

"I am getting stronger."

Then:

"I am discovering new characters."

Then:

"I am discovering their stories."

Then:

"I want to complete this character's journey."

Finally:

"I am playing a game, but I am also learning and remembering
the Biblical story."

That is the core identity of Bible Survivors.