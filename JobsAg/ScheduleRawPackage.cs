namespace NoiseStudio.JobsAg {
    internal struct ScheduleRawPackage {

        public int PackageStartIndex { get; }
        public int PackageEndIndex { get; }

        public ScheduleRawPackage(int packageStartIndex, int packageEndIndex) {
            PackageStartIndex = packageStartIndex;
            PackageEndIndex = packageEndIndex;
        }

    }
}
