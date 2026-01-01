import Image from 'next/image';
import Header from '@/components/header_component';

export default function Home() {
  return (
    <>
      <Header />
      <main className="h-[calc(100%-64px)] flex items-center justify-center relative">
        <div className="container mx-auto flex flex-col md:flex-row items-center justify-between px-4 mt-30">
          {/* Left column - Image */}
          <div className="w-full md:w-1/2 relative h-[400px]">
            <Image
              src="/image/home_image.png"
              alt="Welcome Background"
              fill
              className="object-contain opacity-80"
              priority
            />
          </div>

          {/* Right column - Text */}
          <div className="w-full md:w-1/2 text-center md:text-left space-y-4">
            <h1 className="text-4xl md:text-5xl font-bold text-black">
              Welcome
            </h1>
            <p className="text-2xl md:text-4xl font-bold text-black">
              to
            </p>
            <p className="text-2xl md:text-4xl font-bold text-black">
              Student Attendance System
            </p>
            <p className="text-xl md:text-2xl font-bold text-black">
              Developed by Group 20
            </p>
          </div>
        </div>
      </main>
    </>
  );
}