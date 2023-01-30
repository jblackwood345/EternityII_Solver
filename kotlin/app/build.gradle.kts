plugins {
    // Apply the org.jetbrains.kotlin.jvm Plugin to add support for Kotlin.
    id("org.jetbrains.kotlin.jvm") version "1.8.0"

    // Apply the application plugin to add support for building a CLI application in Java.
    application

    id("org.jmailen.kotlinter") version "3.13.0"
    id("io.gitlab.arturbosch.detekt") version "1.22.0"
}

repositories {
    // Use Maven Central for resolving dependencies.
    mavenCentral()
}

dependencies {
    // Align versions of all Kotlin components
    implementation(platform("org.jetbrains.kotlin:kotlin-bom"))

    // Use the Kotlin JDK 8 standard library.
    implementation("org.jetbrains.kotlin:kotlin-stdlib-jdk8")

    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-core:1.6.4")
}

application {
    // Define the main class for the application.
    mainClass.set("eternityii.AppKt")
}

detekt {
    buildUponDefaultConfig = true
    config = files("../config/detekt/detekt-config.yml")
}

tasks.withType<org.jetbrains.kotlin.gradle.tasks.KotlinCompile>().all {
    kotlinOptions {
        allWarningsAsErrors = true
    }
}
