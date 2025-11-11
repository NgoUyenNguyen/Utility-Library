# State Machine
***Base system to create a state framework, allow seperating runitime logic to different states.***

**[Usage](Usage.md)**

---
## STATE INCLUDES:

### Properties
|Property|Description|
|---|---|
|**[stateKey](StateKey.md)**| A key equivalent to each State for StateManager managing|

### Protected Methods
| Method                                    |Description|
|-------------------------------------------|---|
| **[OnEnter](EnterState.md)**              | Methods called once when a State is entered|
| **[OnUpdate](UpdateState.md)**            |Method called every frame while in the State|
| **[OnExit](ExitState.md)**                |Methods called once when a State is exited|
| **[GetTransition](GenerateNextState.md)** |Method to generate the next state based on the current state logic.|
  
---
## STATEMANAGER INCLUDES:

### Properties
|Property|Description|
|---|---|
|**[currentState](currentState.md)**| The current state of the state machine|

### Events
|Event|Description|
|---|---|
|**[OnStateChanged](OnStateChanged.md)**| Callback fired whenever state changes|

### Public Methods
|Method|Description|
|---|---|
|**[SetNextState](SetNextState.md)**|Method for outer to set the next state of the state machine|

### Protected Methods
|Method|Description|
|---|---|
|**[AddStates](AddStates.md)**|Add states to the state machine|
|**[RemoveStates](RemoveStates.md)**|Remove states from the state machine|
|**[GetState](GetState.md)**|Get state from key|

### Abstract Methods
|Method|Description|
|---|---|
|**[InitializeStates](InitializeStates.md)**|Method to register all states|
|**[InitializeEntryState](InitializeEntryState.md)**|Method to define the entry state|

### Virtual Methods
|Method|Description|
|---|---|
|**[OnAwake](OnAwake.md)**|Method is called in Awake|
|**[OnStart](OnStart.md)**|Method is called in Start|

