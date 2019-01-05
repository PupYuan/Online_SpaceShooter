using System;

public class HandlePlayerEvent
{
    //匹配
    public void OnMatching(Player player)
    {
        MatchingQueue.instance.AddPlayer(player);
        //Scene.instance.AddPlayer(player.id);
    }
	//下线
	public void OnLogout(Player player)
	{
		Scene.instance.DelPlayer(player.id);
	}
}