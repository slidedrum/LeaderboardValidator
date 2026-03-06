using UnityEngine;

namespace LeaderboardValidator
{
    public abstract class LogworthyEvent
    {
        string eventName;
        TimeStamp timeStamp;
        protected LogworthyEvent() 
        {
            timeStamp = new();
            LeaderboardValidator.addEventToList(this);
        }
    }
    public class gainPointsEvent : LogworthyEvent
    {
        int pointsGained;
        Player player;
        int currentPointsMult; //Not sure how to cleanly implement this atm.  but should be logged.
        //this should include the source of what's giving the points somehow. (damage, barrier, power up, etc)  Without major code changes this would be annoying to do as a mod.
        public gainPointsEvent(int pointsGained, Player player) : base()
        {
            this.pointsGained = pointsGained;
            this.player = player;
        }
    }
    public class bloodChangedEvent : LogworthyEvent
    {
        int bloodDelta;
        Player player;
        //this should include the source of what's giving the blood somehow. (damage, debug, power up, etc)  Without major code changes this would be annoying to do as a mod.
        public bloodChangedEvent(int bloodDelta, Player player) : base()
        {
            this.bloodDelta = bloodDelta;
            this.player = player;
        }
    }
    public class fireEvent : LogworthyEvent
    {
        Player player;
        Vector3 playerPos;
        Quaternion playerRoation;
        //Include ray hit?
        public fireEvent(Player player) : base()
        {
            this.player = player;
            playerPos = player.vmPos.transform.position;
            playerRoation = player.vmPos.transform.rotation;
        }
    }
}
