package eternityii

fun main(args: Array<String>) {
    val numCores = if ("cores" in args) {
        args[args.indexOf("cores") + 1].toInt()
    } else {
        Runtime.getRuntime().availableProcessors()
    }

    SolverStarter(numCores).run()
}
