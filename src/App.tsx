import React, { useState } from 'react';
import {
  Download,
  Globe,
  Gauge,
  ListFilter,
  Settings,
  Info,
  ExternalLink,
  CheckCircle2,
  FolderOpen,
  Pause,
  Play,
  RotateCcw,
  Trash2,
  Copy,
  Check,
  ShieldCheck,
  Cpu,
  HardDrive,
  Layers,
  ArrowDownToLine
} from 'lucide-react';

export default function App() {
  const [activeTab, setActiveTab] = useState<'home' | 'browser' | 'downloader' | 'downloads' | 'settings' | 'about'>('home');
  const [copied, setCopied] = useState(false);
  const [simulatedUrl, setSimulatedUrl] = useState('https://www.youtube.com/watch?v=sample');
  const [simulatedAnalyzing, setSimulatedAnalyzing] = useState(false);
  const [simulatedMedia, setSimulatedMedia] = useState<any>(null);

  const repoUrl = 'https://github.com/mdshaheen631-oss/xeno-downloader';
  const actionsUrl = 'https://github.com/mdshaheen631-oss/xeno-downloader/actions';

  const copyCloneCmd = () => {
    navigator.clipboard.writeText(`git clone ${repoUrl}.git`);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleSimulateAnalyze = () => {
    if (!simulatedUrl) return;
    setSimulatedAnalyzing(true);
    setTimeout(() => {
      setSimulatedAnalyzing(false);
      setSimulatedMedia({
        title: 'High Resolution Nature Stream - 4K Wildlife & Landscapes',
        platform: 'Web Stream / YouTube',
        size: '142.5 MB',
        duration: '14:28',
        formats: ['1080p Full HD (MP4)', '720p HD (MP4)', 'Audio Only (MP3 320kbps)']
      });
      setActiveTab('downloader');
    }, 600);
  };

  return (
    <div className="flex h-screen bg-[#0B0E14] text-slate-100 font-sans antialiased select-none overflow-hidden">
      {/* Sidebar Navigation */}
      <aside className="w-64 bg-[#111622] border-r border-[#253046] flex flex-col justify-between shrink-0">
        <div>
          {/* Brand Header */}
          <div className="p-5 border-b border-[#253046] flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-[#00E5FF] to-[#8B5CF6] p-0.5 shadow-lg shadow-cyan-500/20">
              <img
                src="/assets/xeno_logo.jpg"
                alt="Xeno Logo"
                className="w-full h-full object-cover rounded-[10px]"
                onError={(e) => {
                  (e.target as HTMLElement).style.display = 'none';
                }}
              />
            </div>
            <div>
              <h1 className="font-bold text-base tracking-tight text-white flex items-center gap-1.5">
                Xeno Downloader
              </h1>
              <span className="text-[11px] font-semibold text-cyan-400 bg-cyan-950/60 border border-cyan-800/60 px-1.5 py-0.5 rounded">
                v1.0.0 Native x64
              </span>
            </div>
          </div>

          {/* Nav List */}
          <nav className="p-3 space-y-1">
            <button
              onClick={() => setActiveTab('home')}
              className={`w-full flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                activeTab === 'home'
                  ? 'bg-[#1E2A44] text-cyan-400 font-semibold'
                  : 'text-slate-400 hover:text-white hover:bg-[#1A2234]'
              }`}
            >
              <ArrowDownToLine className="w-4 h-4" />
              <span>Home</span>
            </button>

            <button
              onClick={() => setActiveTab('browser')}
              className={`w-full flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                activeTab === 'browser'
                  ? 'bg-[#1E2A44] text-cyan-400 font-semibold'
                  : 'text-slate-400 hover:text-white hover:bg-[#1A2234]'
              }`}
            >
              <Globe className="w-4 h-4" />
              <span>Embedded Browser</span>
            </button>

            <button
              onClick={() => setActiveTab('downloader')}
              className={`w-full flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                activeTab === 'downloader'
                  ? 'bg-[#1E2A44] text-cyan-400 font-semibold'
                  : 'text-slate-400 hover:text-white hover:bg-[#1A2234]'
              }`}
            >
              <Gauge className="w-4 h-4" />
              <span>Downloader</span>
            </button>

            <button
              onClick={() => setActiveTab('downloads')}
              className={`w-full flex items-center justify-between px-3.5 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                activeTab === 'downloads'
                  ? 'bg-[#1E2A44] text-cyan-400 font-semibold'
                  : 'text-slate-400 hover:text-white hover:bg-[#1A2234]'
              }`}
            >
              <div className="flex items-center gap-3">
                <ListFilter className="w-4 h-4" />
                <span>Downloads</span>
              </div>
              <span className="bg-cyan-500/20 text-cyan-300 text-xs px-2 py-0.5 rounded-full font-bold">
                2
              </span>
            </button>

            <div className="my-2 border-t border-[#253046]/70" />

            <button
              onClick={() => setActiveTab('settings')}
              className={`w-full flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                activeTab === 'settings'
                  ? 'bg-[#1E2A44] text-cyan-400 font-semibold'
                  : 'text-slate-400 hover:text-white hover:bg-[#1A2234]'
              }`}
            >
              <Settings className="w-4 h-4" />
              <span>Settings</span>
            </button>

            <button
              onClick={() => setActiveTab('about')}
              className={`w-full flex items-center gap-3 px-3.5 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                activeTab === 'about'
                  ? 'bg-[#1E2A44] text-cyan-400 font-semibold'
                  : 'text-slate-400 hover:text-white hover:bg-[#1A2234]'
              }`}
            >
              <Info className="w-4 h-4" />
              <span>About &amp; Specs</span>
            </button>
          </nav>
        </div>

        {/* Sidebar Footer */}
        <div className="p-4 border-t border-[#253046] bg-[#0C101A]">
          <div className="flex items-center justify-between text-xs text-slate-400 mb-2">
            <span>Engine</span>
            <span className="text-emerald-400 font-semibold flex items-center gap-1">
              <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
              Stream I/O 64KB
            </span>
          </div>
          <a
            href={actionsUrl}
            target="_blank"
            rel="noreferrer"
            className="flex items-center justify-center gap-2 w-full py-2 px-3 bg-gradient-to-r from-cyan-500 to-violet-600 text-slate-950 font-bold rounded-lg text-xs shadow-md shadow-cyan-500/20 hover:opacity-90 transition-opacity"
          >
            <Download className="w-3.5 h-3.5" />
            <span>Download Artifact</span>
          </a>
        </div>
      </aside>

      {/* Main Content Area */}
      <main className="flex-1 flex flex-col overflow-y-auto bg-[#0B0E14]">
        {/* Top GitHub Release Banner */}
        <header className="bg-[#131926] border-b border-[#253046] px-8 py-3.5 flex items-center justify-between shrink-0">
          <div className="flex items-center gap-3">
            <span className="flex h-2.5 w-2.5 rounded-full bg-emerald-400"></span>
            <span className="text-xs font-semibold text-slate-300">
              Native Windows Desktop Solution &amp; GitHub Actions Pipeline Ready
            </span>
          </div>
          <div className="flex items-center gap-3">
            <button
              onClick={copyCloneCmd}
              className="flex items-center gap-1.5 text-xs text-slate-300 bg-[#1A2234] hover:bg-[#253046] px-3 py-1.5 rounded-md border border-[#253046] transition-colors"
            >
              {copied ? <Check className="w-3.5 h-3.5 text-emerald-400" /> : <Copy className="w-3.5 h-3.5" />}
              <span>{copied ? 'Copied' : 'Clone Repo'}</span>
            </button>
            <a
              href={repoUrl}
              target="_blank"
              rel="noreferrer"
              className="flex items-center gap-1.5 text-xs font-semibold text-cyan-400 hover:text-cyan-300 bg-cyan-950/60 hover:bg-cyan-900/60 px-3 py-1.5 rounded-md border border-cyan-800 transition-colors"
            >
              <span>GitHub Repository</span>
              <ExternalLink className="w-3.5 h-3.5" />
            </a>
          </div>
        </header>

        {/* Tab Content */}
        <div className="p-8 max-w-6xl w-full mx-auto">
          {activeTab === 'home' && (
            <div className="space-y-6">
              {/* Hero Banner */}
              <div className="bg-[#161D2B] border border-[#253046] rounded-2xl p-7 relative overflow-hidden">
                <div className="absolute right-0 top-0 w-80 h-80 bg-gradient-to-br from-cyan-500/10 to-violet-600/10 rounded-full blur-3xl pointer-events-none" />
                <div className="flex items-center justify-between gap-6 relative z-10">
                  <div>
                    <h2 className="text-2xl font-bold text-white mb-2">
                      Xeno Downloader for Windows PC
                    </h2>
                    <p className="text-slate-400 text-sm max-w-xl leading-relaxed">
                      Lightweight, native Windows desktop application with zero RAM bloat. Built with C# and .NET 8 WPF, featuring WebView2 embedded browsing and multi-threaded stream downloading.
                    </p>
                  </div>
                  <div className="flex gap-3">
                    <div className="bg-[#101520] border border-[#253046] rounded-xl p-3 text-center min-w-[90px]">
                      <span className="text-[11px] text-slate-400 block mb-0.5">Active</span>
                      <span className="text-xl font-bold text-cyan-400">2</span>
                    </div>
                    <div className="bg-[#101520] border border-[#253046] rounded-xl p-3 text-center min-w-[90px]">
                      <span className="text-[11px] text-slate-400 block mb-0.5">Speed</span>
                      <span className="text-xl font-bold text-emerald-400">12.4 MB/s</span>
                    </div>
                    <div className="bg-[#101520] border border-[#253046] rounded-xl p-3 text-center min-w-[90px]">
                      <span className="text-[11px] text-slate-400 block mb-0.5">Completed</span>
                      <span className="text-xl font-bold text-white">8</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Quick URL Input Card */}
              <div className="bg-[#161D2B] border border-[#253046] rounded-2xl p-6 space-y-4">
                <div>
                  <h3 className="text-base font-semibold text-white">Quick Media Download</h3>
                  <p className="text-xs text-slate-400">Paste any supported video, audio, or media link below to analyze available formats and stream sizes.</p>
                </div>
                <div className="flex gap-3">
                  <input
                    type="text"
                    value={simulatedUrl}
                    onChange={(e) => setSimulatedUrl(e.target.value)}
                    placeholder="Paste media URL here (e.g. https://www.youtube.com/watch?v=...)"
                    className="flex-1 bg-[#101520] border border-[#253046] rounded-xl px-4 py-3 text-sm text-slate-100 placeholder-slate-500 focus:outline-none focus:border-cyan-400 transition-colors"
                  />
                  <button
                    onClick={() => setSimulatedUrl('https://archive.org/details/nasa-video-space-shuttle-launch')}
                    className="px-4 py-3 bg-[#1A2234] hover:bg-[#253046] border border-[#253046] text-slate-200 font-semibold text-sm rounded-xl transition-colors"
                  >
                    Paste Sample
                  </button>
                  <button
                    onClick={handleSimulateAnalyze}
                    disabled={simulatedAnalyzing}
                    className="px-6 py-3 bg-gradient-to-r from-cyan-400 to-cyan-500 text-slate-950 font-bold text-sm rounded-xl hover:opacity-95 transition-opacity flex items-center gap-2"
                  >
                    {simulatedAnalyzing ? (
                      <span className="flex items-center gap-2">
                        <span className="w-4 h-4 border-2 border-slate-950 border-t-transparent rounded-full animate-spin"></span>
                        Analyzing...
                      </span>
                    ) : (
                      <>
                        <Gauge className="w-4 h-4" />
                        <span>Analyze &amp; Download</span>
                      </>
                    )}
                  </button>
                </div>
              </div>

              {/* Quick Bookmarks / Supported Services */}
              <div className="space-y-3">
                <h3 className="text-sm font-semibold text-slate-300">Browse &amp; Stream in Built-In Browser</h3>
                <div className="grid grid-cols-4 gap-4">
                  {[
                    { name: 'YouTube', desc: 'Embedded Browser & Search', color: 'text-red-400' },
                    { name: 'Vimeo', desc: 'Creative Showcase Streams', color: 'text-sky-400' },
                    { name: 'Archive.org', desc: 'Public Domain Movies & Media', color: 'text-cyan-400' },
                    { name: 'Pexels Video', desc: 'Royalty-Free Stock Media', color: 'text-emerald-400' }
                  ].map((service) => (
                    <button
                      key={service.name}
                      onClick={() => setActiveTab('browser')}
                      className="bg-[#161D2B] hover:bg-[#1E273A] border border-[#253046] hover:border-cyan-500/50 rounded-xl p-4 text-left transition-all group"
                    >
                      <span className={`font-bold text-base block mb-1 ${service.color}`}>
                        {service.name}
                      </span>
                      <span className="text-xs text-slate-400 group-hover:text-slate-300">
                        {service.desc}
                      </span>
                    </button>
                  ))}
                </div>
              </div>

              {/* Windows Artifact Download Instructions */}
              <div className="bg-[#131926] border border-cyan-800/40 rounded-2xl p-6">
                <div className="flex items-start gap-4">
                  <div className="p-3 bg-cyan-500/10 border border-cyan-500/30 rounded-xl text-cyan-400">
                    <Download className="w-6 h-6" />
                  </div>
                  <div>
                    <h4 className="font-bold text-white text-base mb-1">
                      Ready to install on Windows PC
                    </h4>
                    <p className="text-slate-400 text-xs leading-relaxed mb-4">
                      The GitHub Actions workflow builds the full Windows x64 application. Once the action finishes, download the artifact from GitHub:
                    </p>
                    <ol className="list-decimal list-inside text-xs text-slate-300 space-y-1.5 mb-4">
                      <li>Go to the <a href={actionsUrl} target="_blank" rel="noreferrer" className="text-cyan-400 underline font-semibold">GitHub Actions</a> tab.</li>
                      <li>Click the workflow run: <strong>Build Xeno Downloader Windows</strong>.</li>
                      <li>Scroll down to the <strong>Artifacts</strong> section at the bottom.</li>
                      <li>Download <strong>Xeno-Downloader-Windows</strong> (contains both the Setup Installer and Portable EXE).</li>
                    </ol>
                    <a
                      href={actionsUrl}
                      target="_blank"
                      rel="noreferrer"
                      className="inline-flex items-center gap-2 px-4 py-2 bg-cyan-500 hover:bg-cyan-400 text-slate-950 font-bold text-xs rounded-lg transition-colors"
                    >
                      <span>Open GitHub Actions Builds</span>
                      <ExternalLink className="w-3.5 h-3.5" />
                    </a>
                  </div>
                </div>
              </div>
            </div>
          )}

          {activeTab === 'browser' && (
            <div className="space-y-4">
              {/* Browser Toolbar Preview */}
              <div className="bg-[#161D2B] border border-[#253046] rounded-2xl p-4 space-y-4">
                <div className="flex items-center justify-between gap-3">
                  <div className="flex items-center gap-2">
                    <button className="p-2 rounded-lg bg-[#1A2234] text-slate-300 hover:bg-[#253046]">←</button>
                    <button className="p-2 rounded-lg bg-[#1A2234] text-slate-300 hover:bg-[#253046]">→</button>
                    <button className="p-2 rounded-lg bg-[#1A2234] text-slate-300 hover:bg-[#253046]">⟳</button>
                  </div>
                  <div className="flex-1 flex items-center bg-[#101520] border border-[#253046] rounded-xl px-3 py-1.5 text-xs text-slate-300 font-mono">
                    <Globe className="w-3.5 h-3.5 text-slate-400 mr-2 shrink-0" />
                    <span className="truncate">https://www.youtube.com</span>
                  </div>
                  <button
                    onClick={() => {
                      setSimulatedMedia({
                        title: 'YouTube Media Stream - In-Browser Detected',
                        platform: 'YouTube',
                        size: '88.4 MB',
                        duration: '08:12',
                        formats: ['1080p Full HD (MP4)', '720p HD (MP4)', 'Audio Only (M4A)']
                      });
                      setActiveTab('downloader');
                    }}
                    className="px-4 py-2 bg-gradient-to-r from-cyan-400 to-cyan-500 text-slate-950 font-bold text-xs rounded-xl hover:opacity-95 transition-opacity flex items-center gap-1.5"
                  >
                    <Download className="w-3.5 h-3.5" />
                    <span>Analyze Current Page</span>
                  </button>
                </div>

                {/* Simulated Embedded Browser Stage */}
                <div className="h-96 bg-[#080B10] border border-[#253046] rounded-xl flex flex-col items-center justify-center p-8 text-center">
                  <div className="w-16 h-16 rounded-2xl bg-red-500/10 border border-red-500/30 flex items-center justify-center text-red-400 mb-4">
                    <Globe className="w-8 h-8" />
                  </div>
                  <h4 className="font-bold text-white text-base mb-1">
                    Integrated Microsoft Edge WebView2 Browser
                  </h4>
                  <p className="text-slate-400 text-xs max-w-md mb-4 leading-relaxed">
                    In the installed Windows desktop software, this region is powered by hardware-accelerated Microsoft WebView2 runtime, allowing you to browse video platforms directly inside Xeno Downloader.
                  </p>
                  <div className="flex gap-2">
                    <span className="text-[11px] bg-[#161D2B] border border-[#253046] text-slate-300 px-3 py-1 rounded-full">
                      Zero DRM Circumvention
                    </span>
                    <span className="text-[11px] bg-[#161D2B] border border-[#253046] text-slate-300 px-3 py-1 rounded-full">
                      Hardware Video Acceleration
                    </span>
                    <span className="text-[11px] bg-[#161D2B] border border-[#253046] text-slate-300 px-3 py-1 rounded-full">
                      Cookie Isolation
                    </span>
                  </div>
                </div>
              </div>
            </div>
          )}

          {activeTab === 'downloader' && (
            <div className="space-y-6">
              <div className="bg-[#161D2B] border border-[#253046] rounded-2xl p-6 space-y-6">
                <div>
                  <h3 className="text-lg font-bold text-white">Media Analysis &amp; Format Selection</h3>
                  <p className="text-xs text-slate-400">Review discovered media streams, select resolution quality, and stream directly to disk.</p>
                </div>

                {/* Media Card */}
                <div className="bg-[#101520] border border-[#253046] rounded-xl p-5 flex gap-5 items-center">
                  <div className="w-40 h-24 rounded-lg bg-[#1A2234] border border-[#253046] flex items-center justify-center text-cyan-400 shrink-0 relative overflow-hidden">
                    <Play className="w-8 h-8 opacity-80" />
                  </div>
                  <div className="flex-1 space-y-2">
                    <span className="text-[11px] font-bold text-cyan-400 bg-cyan-950/60 border border-cyan-800 px-2 py-0.5 rounded">
                      {simulatedMedia?.platform || 'Direct Video Stream'}
                    </span>
                    <h4 className="font-bold text-white text-base">
                      {simulatedMedia?.title || '4K Cinematic Nature Drone Footage (Ultra HD)'}
                    </h4>
                    <div className="flex items-center gap-4 text-xs text-slate-400">
                      <span>Duration: <strong className="text-slate-200">{simulatedMedia?.duration || '12:45'}</strong></span>
                      <span>Estimated Size: <strong className="text-slate-200">{simulatedMedia?.size || '168.2 MB'}</strong></span>
                      <span>Container: <strong className="text-slate-200">MP4 / H.264</strong></span>
                    </div>
                  </div>
                </div>

                {/* Quality Selection */}
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="text-xs font-semibold text-slate-300 block mb-2">Available Quality / Format</label>
                    <select className="w-full bg-[#101520] border border-[#253046] rounded-xl px-4 py-2.5 text-sm text-slate-100 focus:outline-none focus:border-cyan-400">
                      <option>1080p Full HD (MP4) - 168.2 MB</option>
                      <option>720p HD (MP4) - 94.6 MB</option>
                      <option>480p Standard (MP4) - 48.1 MB</option>
                      <option>Audio Only (MP3 320kbps) - 18.2 MB</option>
                    </select>
                  </div>
                  <div>
                    <label className="text-xs font-semibold text-slate-300 block mb-2">Save Destination</label>
                    <div className="flex gap-2">
                      <input
                        type="text"
                        readOnly
                        value="C:\Users\Downloads\Xeno Downloader"
                        className="flex-1 bg-[#101520] border border-[#253046] rounded-xl px-3 py-2 text-xs text-slate-300 font-mono"
                      />
                      <button className="p-2.5 bg-[#1A2234] border border-[#253046] rounded-xl text-slate-200 hover:bg-[#253046]">
                        <FolderOpen className="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                </div>

                {/* Action Bar */}
                <div className="flex items-center justify-between pt-4 border-t border-[#253046]">
                  <span className="text-xs text-slate-400 flex items-center gap-1.5">
                    <ShieldCheck className="w-4 h-4 text-emerald-400" />
                    Streams asynchronously with HTTP Range pause/resumption support.
                  </span>
                  <button
                    onClick={() => setActiveTab('downloads')}
                    className="px-6 py-3 bg-gradient-to-r from-cyan-400 to-cyan-500 text-slate-950 font-bold text-sm rounded-xl hover:opacity-95 transition-opacity flex items-center gap-2"
                  >
                    <Download className="w-4 h-4" />
                    <span>Download to PC</span>
                  </button>
                </div>
              </div>
            </div>
          )}

          {activeTab === 'downloads' && (
            <div className="space-y-6">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="text-xl font-bold text-white">Downloads Manager</h3>
                  <p className="text-xs text-slate-400">Track active downloads, network speeds, pause/resume, and open completed files.</p>
                </div>
                <div className="flex gap-2">
                  <button className="px-3 py-1.5 bg-[#161D2B] hover:bg-[#1E273A] border border-[#253046] rounded-lg text-xs font-semibold text-slate-200 flex items-center gap-1.5">
                    <Pause className="w-3.5 h-3.5" /> Pause All
                  </button>
                  <button className="px-3 py-1.5 bg-[#161D2B] hover:bg-[#1E273A] border border-[#253046] rounded-lg text-xs font-semibold text-slate-200 flex items-center gap-1.5">
                    <Play className="w-3.5 h-3.5 text-cyan-400" /> Resume All
                  </button>
                  <button className="px-3 py-1.5 bg-[#161D2B] hover:bg-[#1E273A] border border-[#253046] rounded-lg text-xs font-semibold text-slate-200 flex items-center gap-1.5">
                    <Trash2 className="w-3.5 h-3.5" /> Clear Completed
                  </button>
                </div>
              </div>

              {/* Download Items */}
              <div className="space-y-3">
                {/* Item 1: Active */}
                <div className="bg-[#161D2B] border border-[#253046] rounded-xl p-4 space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <span className="font-bold text-sm text-white">Nature_Landscape_Documentary_1080p.mp4</span>
                      <span className="text-[10px] font-bold bg-cyan-950/80 border border-cyan-800 text-cyan-400 px-2 py-0.5 rounded">1080p HD</span>
                    </div>
                    <span className="text-xs font-semibold text-cyan-400 bg-cyan-500/10 px-2.5 py-1 rounded-md">Downloading (8.4 MB/s)</span>
                  </div>
                  <div className="w-full bg-[#101520] h-2 rounded-full overflow-hidden">
                    <div className="bg-gradient-to-r from-cyan-400 to-violet-500 h-full w-[68%]" />
                  </div>
                  <div className="flex items-center justify-between text-xs text-slate-400">
                    <div className="flex items-center gap-3">
                      <span className="font-bold text-cyan-400">68.4%</span>
                      <span>114.2 MB / 168.0 MB</span>
                      <span>ETA: 00:06</span>
                    </div>
                    <div className="flex items-center gap-1">
                      <button className="p-1.5 hover:bg-[#253046] rounded-md text-slate-300" title="Pause"><Pause className="w-3.5 h-3.5" /></button>
                      <button className="p-1.5 hover:bg-[#253046] rounded-md text-slate-300" title="Open Folder"><FolderOpen className="w-3.5 h-3.5" /></button>
                      <button className="p-1.5 hover:bg-[#253046] rounded-md text-rose-400" title="Cancel"><Trash2 className="w-3.5 h-3.5" /></button>
                    </div>
                  </div>
                </div>

                {/* Item 2: Active */}
                <div className="bg-[#161D2B] border border-[#253046] rounded-xl p-4 space-y-3">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <span className="font-bold text-sm text-white">Public_Domain_Archive_Film_1952.mp4</span>
                      <span className="text-[10px] font-bold bg-violet-950/80 border border-violet-800 text-violet-400 px-2 py-0.5 rounded">720p</span>
                    </div>
                    <span className="text-xs font-semibold text-emerald-400 bg-emerald-500/10 px-2.5 py-1 rounded-md">Downloading (4.0 MB/s)</span>
                  </div>
                  <div className="w-full bg-[#101520] h-2 rounded-full overflow-hidden">
                    <div className="bg-gradient-to-r from-cyan-400 to-violet-500 h-full w-[34%]" />
                  </div>
                  <div className="flex items-center justify-between text-xs text-slate-400">
                    <div className="flex items-center gap-3">
                      <span className="font-bold text-cyan-400">34.1%</span>
                      <span>42.0 MB / 123.0 MB</span>
                      <span>ETA: 00:20</span>
                    </div>
                    <div className="flex items-center gap-1">
                      <button className="p-1.5 hover:bg-[#253046] rounded-md text-slate-300" title="Pause"><Pause className="w-3.5 h-3.5" /></button>
                      <button className="p-1.5 hover:bg-[#253046] rounded-md text-slate-300" title="Open Folder"><FolderOpen className="w-3.5 h-3.5" /></button>
                      <button className="p-1.5 hover:bg-[#253046] rounded-md text-rose-400" title="Cancel"><Trash2 className="w-3.5 h-3.5" /></button>
                    </div>
                  </div>
                </div>

                {/* Item 3: Completed */}
                <div className="bg-[#161D2B] border border-[#253046] rounded-xl p-4 space-y-3 opacity-80">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <span className="font-bold text-sm text-white">Chill_Synthwave_Beat_AudioOnly.mp3</span>
                      <span className="text-[10px] font-bold bg-emerald-950/80 border border-emerald-800 text-emerald-400 px-2 py-0.5 rounded">MP3 320k</span>
                    </div>
                    <span className="text-xs font-semibold text-emerald-400 flex items-center gap-1">
                      <CheckCircle2 className="w-3.5 h-3.5" /> Completed
                    </span>
                  </div>
                  <div className="w-full bg-[#101520] h-2 rounded-full overflow-hidden">
                    <div className="bg-emerald-500 h-full w-full" />
                  </div>
                  <div className="flex items-center justify-between text-xs text-slate-400">
                    <span>18.4 MB (Finished in 4s)</span>
                    <div className="flex items-center gap-1">
                      <button className="p-1.5 hover:bg-[#253046] rounded-md text-emerald-400" title="Open File"><Play className="w-3.5 h-3.5" /></button>
                      <button className="p-1.5 hover:bg-[#253046] rounded-md text-slate-300" title="Open Folder"><FolderOpen className="w-3.5 h-3.5" /></button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}

          {activeTab === 'settings' && (
            <div className="bg-[#161D2B] border border-[#253046] rounded-2xl p-6 space-y-6">
              <div>
                <h3 className="text-lg font-bold text-white">Application Settings</h3>
                <p className="text-xs text-slate-400">Stored locally in Windows %AppData%\XenoDownloader\settings.json without any remote tracking.</p>
              </div>

              <div className="space-y-4 divide-y divide-[#253046]">
                <div className="pt-2 flex items-center justify-between">
                  <div>
                    <h4 className="text-sm font-semibold text-white">Default Download Directory</h4>
                    <p className="text-xs text-slate-400">Folder where downloads are automatically stored.</p>
                  </div>
                  <span className="text-xs font-mono text-cyan-400 bg-[#101520] px-3 py-1.5 rounded-lg border border-[#253046]">
                    %USERPROFILE%\Downloads\Xeno Downloader
                  </span>
                </div>

                <div className="pt-4 flex items-center justify-between">
                  <div>
                    <h4 className="text-sm font-semibold text-white">Max Concurrent Downloads</h4>
                    <p className="text-xs text-slate-400">Limits simultaneous tasks to preserve bandwidth and eliminate CPU freezes.</p>
                  </div>
                  <span className="text-xs font-bold text-white bg-[#1A2234] px-3 py-1.5 rounded-lg border border-[#253046]">
                    3 simultaneous tasks
                  </span>
                </div>

                <div className="pt-4 flex items-center justify-between">
                  <div>
                    <h4 className="text-sm font-semibold text-white">Clipboard Auto-Detection</h4>
                    <p className="text-xs text-slate-400">Lightweight 2-second periodic poll with zero CPU overhead.</p>
                  </div>
                  <span className="text-xs text-emerald-400 font-semibold bg-emerald-950/60 px-3 py-1 rounded-full border border-emerald-800">
                    Enabled
                  </span>
                </div>

                <div className="pt-4 flex items-center justify-between">
                  <div>
                    <h4 className="text-sm font-semibold text-white">Minimize to System Tray</h4>
                    <p className="text-xs text-slate-400">Keep downloading silently in Windows taskbar notification area.</p>
                  </div>
                  <span className="text-xs text-emerald-400 font-semibold bg-emerald-950/60 px-3 py-1 rounded-full border border-emerald-800">
                    Enabled
                  </span>
                </div>
              </div>
            </div>
          )}

          {activeTab === 'about' && (
            <div className="space-y-6">
              <div className="bg-[#161D2B] border border-[#253046] rounded-2xl p-6 flex gap-6 items-center">
                <div className="w-20 h-20 rounded-2xl bg-gradient-to-tr from-[#00E5FF] to-[#8B5CF6] p-0.5 shadow-xl shadow-cyan-500/20 shrink-0">
                  <img src="/assets/xeno_logo.jpg" alt="Logo" className="w-full h-full object-cover rounded-[14px]" />
                </div>
                <div>
                  <h3 className="text-2xl font-bold text-white mb-1">Xeno Downloader</h3>
                  <p className="text-xs text-cyan-400 font-semibold mb-2">Version 1.0.0 Stable (Windows Desktop x64)</p>
                  <p className="text-xs text-slate-300 leading-relaxed max-w-xl">
                    A dedicated native Windows desktop media management system designed for maximum stability, fast startup, and efficient memory usage on modern and low-end Windows hardware.
                  </p>
                </div>
              </div>

              {/* Architecture Specs */}
              <div className="grid grid-cols-3 gap-4">
                <div className="bg-[#161D2B] border border-[#253046] rounded-xl p-4">
                  <Cpu className="w-5 h-5 text-cyan-400 mb-2" />
                  <h4 className="text-sm font-bold text-white mb-1">.NET 8.0 WPF</h4>
                  <p className="text-xs text-slate-400">Compiled to native x64 binary with zero runtime overhead.</p>
                </div>
                <div className="bg-[#161D2B] border border-[#253046] rounded-xl p-4">
                  <HardDrive className="w-5 h-5 text-violet-400 mb-2" />
                  <h4 className="text-sm font-bold text-white mb-1">Direct-to-Disk Stream</h4>
                  <p className="text-xs text-slate-400">64KB ring buffer with HTTP Range header resume support.</p>
                </div>
                <div className="bg-[#161D2B] border border-[#253046] rounded-xl p-4">
                  <Layers className="w-5 h-5 text-emerald-400 mb-2" />
                  <h4 className="text-sm font-bold text-white mb-1">Edge WebView2</h4>
                  <p className="text-xs text-slate-400">Modern web integration with isolated user data and hardware video.</p>
                </div>
              </div>
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
