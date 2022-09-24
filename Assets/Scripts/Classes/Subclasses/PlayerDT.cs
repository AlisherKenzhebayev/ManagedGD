using System.Collections.Generic;

public class PlayerDT: DamageTaker
{
    private void Update()
    {
        EventManager.TriggerEvent("currentHealthPlayer", new Dictionary<string, object> { { "amount", this.FracHealth } });
    }

    internal override void DoDeath()
    {
        SceneLoaderManager.LoadEnum(SceneLoaderManager.ScenesEnum.DeathScene);
    }
}