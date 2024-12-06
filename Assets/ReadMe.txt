Breath Works VR Experience
A meditation and breathwork VR application designed for users who prefer an active, exploration-based approach to mindfulness and relaxation.

Overview
This VR experience combines mindfulness practices with interactive elements in an explorable virtual environment. Users can learn breathwork techniques through hands-on interaction with various objects and environments, leveraging the Method of Loci (Memory Palace) technique for better retention.

Setup Requirements
Built with Unity 2021.3
XR Interaction Toolkit
Meta Quest or Quest 2/Pro headset
Oculus Integration package from Unity Asset Store

Note: Using different Unity versions may cause compatibility issues.

Features

Core Systems:
	Interactive breathwork tutorials
	Memory Palace-based learning environments
	Progressive relaxation exercises
	Multiple interaction methods (grab, touch, swipe)

Interaction Systems

	1. Movement & Navigation
		Teleportation system
		Elevator movement
		Climbing system
	2. Object Interaction
		Grab and manipulation system
	    Physics-based interactions
	    Scanner mechanics with three variations
	    Card reader and security systems
	3. Environment Features
		Dynamic door systems 
		Interactive dispensers
		Number pad security system
		Customizable spawn points

Implementation Details

My Key Scripts
ElevatorPlayerParent.cs: Handles player movement in elevators
GrabTriggerAction.cs: Core grabbing mechanics
AdvancedPrefabDispenser.cs: Object spawning system
CardReader.cs: Security access system
NumberPad.cs: Interactive keypad system
GameManager.cs: Handles core game mechanics including scoring system, streak tracking, and high score persistence
Keycard.cs: Implements VR-compatible grabbable keycard functionality with physics interactions
KeycardSpawner.cs: Controls the spawning and animation of keycards from dispenser points
ScannableObject.cs: Comprehensive system for objects that can be scanned, with events, limitations and visual effects management
StreakEffectsManager.cs: Advanced effects system managing visual and audio feedback for scoring streaks with object pooling
SwipeZone.cs: Detects keycard swipe interactions and communicates with the card reader system
TouchButton.cs: Tactile button interactions
FloatingLantern.cs: Manages interactive lantern objects that float upward when triggered by flame or proximity

Custom-Built Elements:
Main building structure
Elevator system
Prefab dispenser
Locked safe system
Sandbox environment
Closets and closet doors
Materials and textures
Sound effects

Asset Attribution

SketchFab Assets:
“Paper lantern_3” (https://skfb.ly/ovvBX) by Sparrow is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
"Chess Board" (https://skfb.ly/6BDGq) by Yanez Designs is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
"The Steampunk Vault" (https://skfb.ly/opGq9) by Ljm 3D is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
"Animated Old Chest" (https://skfb.ly/IYux) by NOT_Lonely is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
"Buster Drone" (https://skfb.ly/TBnX) by LaVADraGoN is licensed under Creative Commons Attribution-NonCommercial (http://creativecommons.org/licenses/by-nc/4.0/).
"Vintage Boombox" (https://skfb.ly/6R9vQ) by rsboros is licensed under Creative Commons Attribution-NonCommercial (http://creativecommons.org/licenses/by-nc/4.0/).
"Book" (https://skfb.ly/6TJRN) by zoging is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

Unity Pathway Assets:
Environmental elements (waterfall, pool, water bodies)
Interactive objects (cubes, pyramids, torus)
Basic primitives
Example assets (candles, ribbon, piggy banks, mallet)
Door frame
Key unlock system
Button interfaces

Unity Asset Store:
Mesh Effects by kripto289 
HQ Rocks by Next Level 3D

Daz Assets:
Rooftop glass room
Downstairs Living Room
Backyard deck and furniture
Treehouse structure (modified balcony)

Educational Content Referenced videos:
5 Ways To Improve Your Breathing by James Nestor https://youtu.be/f6yAY1oZUOA?si=uoODi3zyBRpLCbTb
Box Breathing by the University of Alabama at Birmingham https://youtu.be/bF_1ZiFta-E?si=QTSo9-5zqbSVrHqD
Vagus Nerve Stimulation by The Holistic Psychologist https://youtu.be/si5oAbov0VI?si=ssu8Ke9yQsdZZSvC
The Psychological Sigh by Dr. Andrew Huberman https://youtube.com/shorts/9JhTMTksk9s?si=jtSSn01_-WH4jdqp

Development Notes:
This project was developed as part of the Unity Pathway program, implementing various VR interaction techniques and systems. The current build represents a slice of the full application's potential features.


