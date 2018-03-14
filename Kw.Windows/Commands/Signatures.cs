namespace Kw.Windows.Commands
{
    //  ReSharper disable once InconsistentNaming
    namespace Signatures
	{
		public abstract class UICommandSignature { }

		public abstract class New : UICommandSignature { }
		public abstract class Open : UICommandSignature { }
		public abstract class Save : UICommandSignature { }
		public abstract class SaveAs : UICommandSignature { }

		public abstract class Print : UICommandSignature { }
		
		public abstract class Exit : UICommandSignature { }
	}
}

