public class TriggerBoolean
{
    private bool m_state;

    public void Set() => m_state = true;

    public bool TryConsume()
    {
        bool cache = m_state;
        m_state = false;
        return cache;
    }
}
