VAR all_checked = 0

Hey.

-> menu

=== menu ===
{all_checked >= 3:
    * [So what is this about?] -> tutorial_customers
}
* [Who are you?]
    ...
    You really don't remember..?
    Let's leave those questions for later.
    I shall explain for you everything here.
    ~ all_checked += 1
    -> menu
* [Where am I?]
    The Inn.
    The Gemini Inn.
    ~ all_checked += 1
    -> menu
* [Who am I?]
    Tavern keeper.
    Quest giver.
    You are inconsequential.
    ~ all_checked += 1
    -> menu

= tutorial_customers
Your customers. #show_customers
There's nothing interesting about them, really.
But they have their use, so treat them well.
Or don't. Up to you.
Now open the map. #show_hud
* [Open the map] -> tutorial_quest

= tutorial_quest
You first request. #spawn_quest
Be cautious. Some of them don't live long.
After all, patience is limited. Same goes for lives.
* [Check quest] -> tutorial_abilities

= tutorial_abilities
The quests may require abilities & stats. #show_quest
You could choose Travelers with such skills or not, your choice.
Forget about the quest for now.
* [Dismiss quest] -> tutorial_end

= tutorial_end
This is it. This is your job. #hide_quest
At last...
Everything you're doing here is your choice.
You can run from this fact, but in the end you'll face it.
Again.
And again.
And again...
I'll be here. We will talk again.
It's time.

* [Get to work]

-> END