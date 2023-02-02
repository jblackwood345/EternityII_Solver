package eternityii

fun Long.fmt(): String =
    this.toString()
        .reversed()
        .chunked(3)
        .joinToString(",")
        .reversed()
