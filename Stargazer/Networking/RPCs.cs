namespace Stargazer.Networking;

public enum RPC : uint
{
    // for the Yeehaw sfx 
    Yeehaw = 1,

    // for the Seeker spawn sfx
    SeekerScream = 2,

    // for changing the bodytype of a player
    ChangeBodyType = 3,

    //For placing lanterns as Lightener
    LightUp = 4,
    
    //For Acting as Actor 
    Act = 5,
    
    Silence = 6,
    
    Vacuum = 7,
    
    TriggerGhostTrap = 8,
    
    PlaceGhostTrap = 9,
    
    UseAbility = 10,
    
    Revive = 11,
    
    Sleep = 12,
    
    ThrowShuriken = 13,
    
    UseWildCard = 14,
    
    DisconnectVent = 15,
    SpawnFloristTrap = 16
}