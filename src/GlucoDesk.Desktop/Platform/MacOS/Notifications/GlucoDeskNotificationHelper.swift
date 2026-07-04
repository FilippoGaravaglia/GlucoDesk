import Cocoa
import UserNotifications

let logPath = "/tmp/glucodesk-notification-helper.log"

func log(_ message: String) {
    let line = "\(Date()) \(message)\n"

    if FileManager.default.fileExists(atPath: logPath),
       let handle = FileHandle(forWritingAtPath: logPath) {
        handle.seekToEndOfFile()
        handle.write(line.data(using: .utf8)!)
        handle.closeFile()
        return
    }

    try? line.write(toFile: logPath, atomically: true, encoding: .utf8)
}

final class AppDelegate: NSObject, NSApplicationDelegate, UNUserNotificationCenterDelegate {
    private let title: String
    private let body: String
    private let subtitle: String

    override init() {
        let arguments = CommandLine.arguments

        self.title = arguments.count > 1 ? arguments[1] : "GlucoDesk"
        self.body = arguments.count > 2 ? arguments[2] : "Native glucose awareness notification."
        self.subtitle = arguments.count > 3 ? arguments[3] : "Glucose awareness"

        super.init()
    }

    func applicationDidFinishLaunching(_ notification: Notification) {
        _ = notification

        log("START")
        log("BUNDLE_ID: \(Bundle.main.bundleIdentifier ?? "nil")")
        log("TITLE: \(title)")
        log("SUBTITLE: \(subtitle)")

        NSApp.setActivationPolicy(.accessory)

        let center = UNUserNotificationCenter.current()
        center.delegate = self

        center.requestAuthorization(options: [.alert, .sound, .badge]) { granted, error in
            if let error {
                log("AUTH_ERROR: \(error.localizedDescription)")
                self.terminateSoon()
                return
            }

            if !granted {
                log("AUTH_DENIED")
                self.terminateSoon()
                return
            }

            log("AUTH_GRANTED")
            self.sendNotification()
        }
    }

    private func sendNotification() {
        let content = UNMutableNotificationContent()
        content.title = title
        content.subtitle = subtitle
        content.body = body
        content.sound = UNNotificationSound.default

        let request = UNNotificationRequest(
            identifier: UUID().uuidString,
            content: content,
            trigger: UNTimeIntervalNotificationTrigger(timeInterval: 1, repeats: false))

        UNUserNotificationCenter.current().add(request) { error in
            if let error {
                log("ADD_ERROR: \(error.localizedDescription)")
            } else {
                log("NOTIFICATION_SCHEDULED")
            }

            self.terminateSoon()
        }
    }

    private func terminateSoon() {
        DispatchQueue.main.asyncAfter(deadline: .now() + 3) {
            log("END")
            NSApp.terminate(nil)
        }
    }

    func userNotificationCenter(
        _ center: UNUserNotificationCenter,
        willPresent notification: UNNotification,
        withCompletionHandler completionHandler: @escaping (UNNotificationPresentationOptions) -> Void) {
        _ = center
        _ = notification

        if #available(macOS 11.0, *) {
            completionHandler([.banner, .sound])
        } else {
            completionHandler([.alert, .sound])
        }
    }
}

let app = NSApplication.shared
let delegate = AppDelegate()
app.delegate = delegate
app.run()
