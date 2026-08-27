# Narrative Design

## Purpose

Bible Survivors should let a player experience a Biblical character's journey
as a coherent sequence, not as randomly shuffled facts. Narrative is a later
system that layers on top of a completed gameplay foundation. No narrative
runtime, content, scripture database, quiz, or audio asset exists in the
repository at Phase 0.

## Narrative principles

1. Story chronology is authoritative. Gameplay randomness must not move a
   character from an early chapter to a later chapter.
2. Encounters are character- and chapter-scoped. They are never selected from
   a global unfiltered pool.
3. A story event gives context before asking for reflection or a quiz.
4. Scripture is stored and presented as structured references. Translation
   text is not hard-coded into C#.
5. Rewards and achievements follow recorded completion outcomes, not a
   transient UI event.
6. Audio narration is optional and replaceable. No third-party text-to-speech
   provider becomes a core runtime dependency.

## Intended player flow

    Complete run milestone or victory condition
    → evaluate the active character's eligible current chapter
    → select an eligible encounter from that chapter only
    → present narrative and scripture references
    → optionally play narration / subtitles
    → complete optional quiz
    → record outcome and apply defined progression

The initial implementation must keep the above as an explicit state sequence.
It must not select an encounter during combat merely because a random timer
fires, and it must not permit a future chapter to appear before its
prerequisites are complete.

## Future content model

The following resource-level concepts are planned for their respective
roadmap phases. They are definitions, not Phase 0 classes or files.

| Concept | Required responsibility |
| --- | --- |
| StoryArcDefinition | identifies one character's ordered story arc |
| StoryChapterDefinition | supplies stable chapter id, order, title, prerequisites, and encounter pool |
| EncounterDefinition | identifies a chapter event, eligibility, difficulty, narrative payload, scripture references, quiz/reward links |
| ScriptureReference | book, chapter, verse start, verse end, translation identifier |
| QuizDefinition | comprehension questions, correct responses, difficulty, and result policy |
| AchievementDefinition | achievement metadata and its progression connection |
| Narration payload | paragraphs, optional subtitles, and replaceable audio-asset reference |

The model intentionally separates a narrative definition from the player's
completion record. Definitions describe what can happen. Persistent
progression records what did happen for a particular player/profile.

## Chronology and selection rules

For a given active character:

1. Determine the earliest incomplete chapter whose prerequisites are
   satisfied.
2. Build the encounter candidate set using only that chapter's encounter
   definitions.
3. Filter candidates by their own prerequisites and completion rules.
4. If more than one remains, randomize only inside this filtered set.
5. Record the selected encounter and completion result before calculating
   downstream rewards or unlocks.
6. Advance chapter eligibility only using explicit content prerequisites.

This makes randomness a replayability tool while preserving a linear story
arc. A completed encounter may be replayable only if its content definition
states the policy; replay availability must not automatically grant duplicate
one-time rewards.

## Character eras and narrative

Character eras are content-driven progression states, not separate hard-coded
characters. A future era may enable a range of chapters, provide a new
loadout, alter presentation, or require specific achievements. The narrative
system owns ordering and eligibility; generic combat systems receive only the
neutral runtime data selected by the character/era definition.

## Scripture policy

A scripture reference requires at least:

| Field | Example shape |
| --- | --- |
| Book | stable canonical book identifier |
| Chapter | positive chapter number |
| VerseStart | positive verse number |
| VerseEnd | optional inclusive end verse |
| Translation | stable translation identifier |

References should use canonical identifiers internally and localized display
names at presentation time. Source provenance and translation rights must be
recorded before full text is imported or distributed. During development,
placeholder or reference-only content is acceptable; copied translation text
is not.

## Narrative presentation

Major events are intended to feel like short audio-book chapters of roughly
five minutes, with an introduction, context, conflict, development, climax,
conclusion, and scripture connection. They should not be written as detached
encyclopedia entries or unrelated trivia.

The UI later needs to support narration text, paragraph progression, optional
subtitles, scripture-reference presentation, pause/continue state, and
completion state. Presentation will consume narrative state; it must not
silently decide chapter progression.

## Quiz policy

An event may end in a five-question comprehension quiz. Questions should test
the delivered narrative and cited scripture references, and difficulty should
track the event's narrative and gameplay stage. Passing and failure behavior
must be content-defined and persisted. Exact scoring, retry, and reward
policies are deferred to the quiz phase.

## Phase 0 non-goals

This document does not create stories, choose the first character, write
scripture text, define quiz questions, produce audio, or implement the
encounter flow. It sets the constraints those future implementations must
respect.
