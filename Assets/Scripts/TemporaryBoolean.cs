public class TemporaryBoolean
{
    private bool m_state;
    private float m_timeTillExpiry;
    private bool m_expiredThisTick;

    public bool IsTrue { get => m_state; }

    public TemporaryBoolean()
    {
        m_state = false;
        m_timeTillExpiry = 0;
    }

    public void SetActive(float time)
    {
        m_state = true;
        m_timeTillExpiry = time;
    }

    public void Tick(float time)
    {
        if (m_expiredThisTick) m_expiredThisTick = false;
        if (!m_state) return;

        m_timeTillExpiry -= time;

        if (m_timeTillExpiry < 0)
        {
            Expire();
        }
    }

    public void Expire()
    {
        m_timeTillExpiry = 0f;
        m_state = false;
        m_expiredThisTick = true;
    }

    public bool ExpiredThisTick() => m_expiredThisTick;

    public override string ToString()
    {
        return $"Value: {m_state}, Expiry: {m_timeTillExpiry}";
    }
}
