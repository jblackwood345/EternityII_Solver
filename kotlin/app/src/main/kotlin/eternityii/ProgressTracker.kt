package eternityii

import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.delay
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch

/**
 * Track progress of something in real time.
 *
 * The caller must cancel it when finished.
 */
class ProgressTracker(
    private val jobFunction: (Long) -> Unit,
    private val repeatEveryMs: Long = 30_000L
) {
    private val startTimeMs = System.currentTimeMillis()

    private val job =
        CoroutineScope(Dispatchers.Default).launch {
            while (isActive) {
                delay(repeatEveryMs)
                jobFunction.invoke(elapsedTimeSeconds)
            }
        }

    val elapsedTimeSeconds: Long
        get() = (System.currentTimeMillis() - startTimeMs) / 1000

    fun cancel() {
        job.cancel()
    }
}
