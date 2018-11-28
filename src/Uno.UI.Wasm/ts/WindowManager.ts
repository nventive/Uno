﻿namespace Uno.UI {

	export class WindowManager {

		public static current: WindowManager;
		private static _isHosted: boolean = false;

		/**
		 * Defines if the WindowManager is running in hosted mode, and should skip the
		 * initialization of WebAssembly, use this mode in conjuction with the Uno.UI.WpfHost
		 * to improve debuggability.
		 */
		public static get isHosted(): boolean {
			return WindowManager._isHosted;
		}

		private static readonly unoRootClassName = "uno-root-element";
		private static readonly unoUnarrangedClassName = "uno-unarranged";

		/**
			* Initialize the WindowManager
			* @param containerElementId The ID of the container element for the Xaml UI
			* @param loadingElementId The ID of the loading element to remove once ready
			*/
		public static init(localStoragePath: string, isHosted: boolean, containerElementId: string = "uno-body", loadingElementId: string = "uno-loading"): string {

			if (WindowManager.assembly) {
				throw "Already initialized";
			}

			WindowManager._isHosted = isHosted;

			WindowManager.initMethods();
			HtmlDom.initPolyfills();
			WindowManager.setupStorage(localStoragePath);

			this.current = new WindowManager(containerElementId, loadingElementId);
			this.current.init();

			return "ok";
		}

		private containerElement: HTMLDivElement;
		private rootContent: HTMLElement;

		private allActiveElementsById: { [name: string]: HTMLElement | SVGElement } = {};

		static assembly: UI.Interop.IMonoAssemblyHandle;
		private static resizeMethod: UI.Interop.IMonoMethodHandle;
		private static dispatchEventMethod: UI.Interop.IMonoMethodHandle;

		private constructor(private containerElementId: string, private loadingElementId: string) {
			this.initDom();
		}

		/**
		 * Setup the storage persistence
		 *
		 * */
		static setupStorage(localStoragePath: string): void {
			if (WindowManager.isHosted) {
				console.debug("Hosted Mode: skipping IndexDB initialization");
			}
			else {
				if (WindowManager.isIndexDBAvailable()) {

					FS.mkdir(localStoragePath);
					FS.mount(IDBFS, {}, localStoragePath);

					FS.syncfs(true,
						err => {
							if (err) {
								console.error(`Error synchronizing filsystem from IndexDB: ${err}`);
							}
						}
					);

					window.addEventListener(
						"beforeunload",
						() => WindowManager.synchronizeFileSystem()
					);

					setInterval(() => WindowManager.synchronizeFileSystem(), 10000);
				}
				else {
					console.warn("IndexedDB is not available (private mode?), changed will not be persisted.");
				}
			}
		}

		/**
		 * Determine if IndexDB is available, some browsers and modes disable it.
		 * */
		static isIndexDBAvailable(): boolean {
			try {
				// IndexedDB may not be available in private mode
				window.indexedDB;
				return true;
			} catch (err) {
				return false;
			}
		}

		/**
		 * Synchronize the IDBFS memory cache back to IndexDB
		 * */
		static synchronizeFileSystem(): void {
			FS.syncfs(
				err => {
					if (err) {
						console.error(`Error synchronizing filsystem from IndexDB: ${err}`)
					}
				}
			)
		}

		/**
			* Creates the UWP-compatible splash screen
			* 
			*/
		static setupSplashScreen(): void {

			if (UnoAppManifest && UnoAppManifest.splashScreenImage) {

				const loading = document.getElementById("loading");

				if (loading) {
					loading.remove();
				}

				const unoBody = document.getElementById("uno-body");

				if (unoBody) {
					const unoLoading = document.createElement("div");
					unoLoading.id = "uno-loading";

					if (UnoAppManifest.splashScreenColor) {
						const body = document.getElementsByTagName("body")[0];
						body.style.backgroundColor = UnoAppManifest.splashScreenColor;
					}

					const unoLoadingSplash = document.createElement("div");
					unoLoadingSplash.id = "uno-loading-splash";
					unoLoadingSplash.classList.add("uno-splash");
					unoLoadingSplash.style.backgroundImage = `url('${UnoAppManifest.splashScreenImage}')`;

					unoLoading.appendChild(unoLoadingSplash);

					unoBody.appendChild(unoLoading);
				}
			}
		}

		/**
			* Reads the window's search parameters
			* 
			*/
		static findLaunchArguments(): string {
			if (typeof URLSearchParams === "function") {
				return new URLSearchParams(window.location.search).toString();
			}
			else {
				let queryIndex = document.location.search.indexOf('?');

				if (queryIndex != -1) {
					return document.location.search.substring(queryIndex + 1);
				}

				return "";
			}
		}

		/**
			* Create a html DOM element representing a Xaml element.
			*
			* You need to call addView to connect it to the DOM.
			*/
		public createContent(contentDefinition: IContentDefinition): string {
			this.createContentInternal(contentDefinition);

			return "ok";
		}

		/**
			* Create a html DOM element representing a Xaml element.
			*
			* You need to call addView to connect it to the DOM.
			*/
		public createContentNative(pParams: number): boolean {

			var params = WindowManagerCreateContentParams.unmarshal(pParams);

			var def = <IContentDefinition> {
				id: params.HtmlId,
				handle: params.Handle,
				isFocusable: params.IsFocusable,
				isFrameworkElement: params.IsFrameworkElement,
				isSvg: params.IsSvg,
				tagName: params.TagName,
				type: params.Type,
				classes: params.Classes
			};

			this.createContentInternal(def);

			return true;
		}

		private createContentInternal(contentDefinition: IContentDefinition): void {
			// Create the HTML element
			const element =
				contentDefinition.isSvg
					? document.createElementNS("http://www.w3.org/2000/svg", contentDefinition.tagName)
					: document.createElement(contentDefinition.tagName);
			element.id = String(contentDefinition.id);
			element.setAttribute("XamlType", contentDefinition.type);
			element.setAttribute("XamlHandle", `${contentDefinition.handle}`);
			if (contentDefinition.isFrameworkElement) {
				element.classList.add(WindowManager.unoUnarrangedClassName);
			}
			if (element.hasOwnProperty("tabindex")) {
				(element as any)["tabindex"] = contentDefinition.isFocusable ? 0 : -1;
			} else {
				element.setAttribute("tabindex", contentDefinition.isFocusable ? '0' : '-1');
			}

			if (contentDefinition) {
				for (const className of contentDefinition.classes) {
					element.classList.add(`uno-${className}`);
				}
			}

			// Add the html element to list of elements
			this.allActiveElementsById[contentDefinition.id] = element;
		}

		/**
			* Set a name for an element.
			*
			* This is mostly for diagnostic purposes.
			*/
		public setName(elementId: number, name: string): string {
			this.setNameInternal(elementId, name);
			return "ok";
		}

		/**
			* Set a name for an element.
			*
			* This is mostly for diagnostic purposes.
			*/
		public setNameNative(pParam: number): boolean {
			let params = WindowManagerSetNameParams.unmarshal(pParam);
			this.setNameInternal(params.HtmlId, params.Name);
			return true;
		}

		private setNameInternal(elementId: number, name: string): void {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			htmlElement.setAttribute("XamlName", name);
		}

		/**
			* Set an attribute for an element.
			*/
		public setAttribute(elementId: string, attributes: { [name: string]: string }): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}


			for (const name in attributes) {
				if (attributes.hasOwnProperty(name)) {
					htmlElement.setAttribute(name, attributes[name]);
				}
			}

			return "ok";
		}

		/**
			* Set an attribute for an element.
			*/
		public setAttributeNative(pParams: number): boolean {

			let params = WindowManagerSetAttributeParams.unmarshal(pParams);

			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[params.HtmlId];
			if (!htmlElement) {
				throw `Element id ${params.HtmlId} not found.`;
			}

			for (var i = 0; i < params.Pairs_Length; i += 2) {
				htmlElement.setAttribute(params.Pairs[i], params.Pairs[i + 1]);
			}

			return true;
		}

		/**
			* Get an attribute for an element.
			*/
		public getAttribute(elementId: string, name: string): any {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			return htmlElement.getAttribute(name);
		}

		/**
			* Set a property for an element.
			*/
		public setProperty(elementId: string, properties: { [name: string]: string }): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			for (const name in properties) {
				if (properties.hasOwnProperty(name)) {
					(htmlElement as any)[name] = properties[name];
				}
			}

			return "ok";
		}

		/**
			* Set a property for an element.
			*/
		public setPropertyNative(pParams:number): boolean {

			let params = WindowManagerSetPropertyParams.unmarshal(pParams);

			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[params.HtmlId];
			if (!htmlElement) {
				throw `Element id ${params.HtmlId} not found.`;
			}

			for (var i = 0; i < params.Pairs_Length; i += 2) {
				(htmlElement as any)[params.Pairs[i]] = params.Pairs[i + 1];
			}

			return true;
		}

		/**
			* Get a property for an element.
			*/
		public getProperty(elementId: string, name: string): any {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			return (htmlElement as any)[name] || "";
		}

		/**
			* Set the CSS style of a html element.
			*
			* To remove a value, set it to empty string.
			* @param styles A dictionary of styles to apply on html element.
			*/
		public setStyle(elementId: string, styles: { [name: string]: string }, setAsArranged: boolean = false): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			for (const style in styles) {
				if (styles.hasOwnProperty(style)) {
					htmlElement.style.setProperty(style, styles[style]);
				}
			}

			if (setAsArranged) {
				htmlElement.classList.remove(WindowManager.unoUnarrangedClassName);
			}

			return "ok";
		}

		/**
		* Set the CSS style of a html element.
		*
		* To remove a value, set it to empty string.
		* @param styles A dictionary of styles to apply on html element.
		*/
		public setStyleNative(pParams:number): boolean {

			let params = WindowManagerSetStylesParams.unmarshal(pParams);

			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[params.HtmlId];
			if (!htmlElement) {
				throw `Element id ${params.HtmlId} not found.`;
			}

			for (var i = 0; i < params.Pairs_Length; i+=2) {
				let key = params.Pairs[i];
				let value = params.Pairs[i+1];

				htmlElement.style.setProperty(key, value);
			}

			if (params.SetAsArranged) {
				htmlElement.classList.remove(WindowManager.unoUnarrangedClassName);
			}

			return true;
		}

		/**
			* Set the CSS style of a html element.
			*
			* To remove a value, set it to empty string.
			* @param styles A dictionary of styles to apply on html element.
			*/
		public resetStyle(elementId: number, names: string[]): string {
			this.resetStyleInternal(elementId, name);
			return "ok";
		}

		/**
			* Set the CSS style of a html element.
			*
			* To remove a value, set it to empty string.
			* @param styles A dictionary of styles to apply on html element.
			*/
		public resetStyleNative(pParams: number): boolean {
			let params = WindowManagerResetStyleParams.unmarshal(pParams);
			this.resetStyleInternal(params.HtmlId, params.Styles);
			return true;
		}

		private resetStyleInternal(elementId: number, names: string[]): void {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			for(const name of names) {
				htmlElement.style.setProperty(name, "");
			}
		}

		/**
			* Load the specified URL into a new tab or window
			* @param url URL to load
			* @returns "True" or "False", depending on whether a new window could be opened or not
			*/
		public open(url: string): string {
			const newWindow = window.open(url, "_blank");

			return newWindow != null
				? "True"
				: "False";
		}

		/**
			* Issue a browser alert to user
			* @param message message to display
			*/
		public alert(message: string): string {
			window.alert(message);

			return "ok";
		}

		/**
			* Sets the browser window title
			* @param message the new title
			*/
		public setWindowTitle(title: string): string {
			document.title = title || UnoAppManifest.displayName;
			return "ok";
		}

		/**
			* Gets the currently set browser window title
			*/
		public getWindowTitle(): string {
			return document.title || UnoAppManifest.displayName;
		}

		/**
			* Add an event handler to a html element.
			*
			* @param eventName The name of the event
			* @param onCapturePhase true means "on trickle down", false means "on bubble up". Default is false.
			*/
		public registerEventOnView(
			elementId: number,
			eventName: string,
			onCapturePhase: boolean = false,
			eventFilterName?: string,
			eventExtractorName?: string
		): string {
			this.registerEventOnViewInternal(elementId, eventName, onCapturePhase, eventFilterName, eventExtractorName);
			return "ok";
		}

		/**
			* Add an event handler to a html element.
			*
			* @param eventName The name of the event
			* @param onCapturePhase true means "on trickle down", false means "on bubble up". Default is false.
			*/
		public registerEventOnViewNative(
			pParams: number
		): boolean {
			let params = WindowManagerRegisterEventOnViewParams.unmarshal(pParams);

			this.registerEventOnViewInternal(params.HtmlId, params.EventName, params.OnCapturePhase, params.EventFilterName, params.EventExtractorName);
			return true;
		}

		/**
			* Add an event handler to a html element.
			*
			* @param eventName The name of the event
			* @param onCapturePhase true means "on trickle down", false means "on bubble up". Default is false.
			*/
		private registerEventOnViewInternal(
			elementId: number,
			eventName: string,
			onCapturePhase: boolean = false,
			eventFilterName?: string,
			eventExtractorName?: string
		): void {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			const eventFilter = this.getEventFilter(eventFilterName);
			const eventExtractor = this.getEventExtractor(eventExtractorName);

			const eventHandler = (event: Event) => {
				if (eventFilter && !eventFilter(event)) {
					return;
				}

				const eventPayload =
					eventExtractor
						? `${eventExtractor(event)}`
						: "";

				var handled = this.dispatchEvent(htmlElement, eventName, eventPayload);
				if (handled) {
					event.stopPropagation();
					if (event instanceof KeyboardEvent) {
						event.preventDefault();
					}
				}
			};

			htmlElement.addEventListener(eventName, eventHandler, onCapturePhase);
		}

		/**
		 * left pointer event filter to be used with registerEventOnView
		 * @param evt
		 */
		private leftPointerEventFilter(evt: any): boolean {
			return evt ? (!evt.button || evt.button == 0) : false;
		}

		/**
		 * pointer event extractor to be used with registerEventOnView
		 * @param evt
		 */
		private pointerEventExtractor(evt: any): string {
			return evt
				? `${evt.pointerId};${evt.clientX};${evt.clientY};${(evt.ctrlKey ? "1" : "0")};${(evt.shiftKey ? "1" : "0")};${evt.button};${evt.pointerType}`
				: "";
		}

		/**
		 * keyboard event extractor to be used with registerEventOnView
		 * @param evt
		 */
		private keyboardEventExtractor(evt: any): string {
			return (evt instanceof KeyboardEvent) ? evt.key : "0";
		}

		/**
		 * Gets the event filter function. See UIElement.HtmlEventFilter
		 * @param eventFilterName an event filter name.
		 */
		private getEventFilter(eventFilterName: string): any {

			if (eventFilterName) {
				switch (eventFilterName) {
					case "LeftPointerEventFilter":
						return this.leftPointerEventFilter;
				}

				throw `Event filter ${eventFilterName} is not supported`;
			}

			return null;
		}

		/**
		 * Gets the event extractor function. See UIElement.HtmlEventExtractor
		 * @param eventExtractorName an event extractor name.
		 */
		private getEventExtractor(eventExtractorName: string): any {

			if (eventExtractorName) {
				switch (eventExtractorName) {
					case "PointerEventExtractor":
						return this.pointerEventExtractor;

					case "KeyboardEventExtractor":
						return this.keyboardEventExtractor;
				}

				throw `Event filter ${eventExtractorName} is not supported`;
			}

			return null;
		}

		/**
			* Set or replace the root content element.
			*/
		public setRootContent(elementId?: string): string {
			if (this.rootContent && this.rootContent.id === elementId) {
				return null; // nothing to do
			}

			if (this.rootContent) {
				// Remove existing
				this.containerElement.removeChild(this.rootContent);

				this.dispatchEvent(this.rootContent, "unloaded");
				this.rootContent.classList.remove(WindowManager.unoRootClassName);
			}

			if (!elementId) {
				return null;
			}

			// set new root
			const newRootElement = this.allActiveElementsById[elementId] as HTMLElement;
			newRootElement.classList.add(WindowManager.unoRootClassName);

			this.rootContent = newRootElement;

			this.dispatchEvent(this.rootContent, "loading");

			this.containerElement.appendChild(this.rootContent);

			this.dispatchEvent(this.rootContent, "loaded");
			newRootElement.classList.remove(WindowManager.unoUnarrangedClassName); // patch because root is not measured/arranged

			this.resize();

			return "ok";
		}

		/**
			* Set a view as a child of another one.
			*
			* "Loading" & "Loaded" events will be raised if nescessary.
			*
			* @param index Position in children list. Appended at end if not specified.
			*/
		public addView(parentId: number, childId: number, index?: number): string {
			this.addViewInternal(parentId, childId, index);
			return "ok";
		}

		/**
			* Set a view as a child of another one.
			*
			* "Loading" & "Loaded" events will be raised if nescessary.
			*
			* @param pParams Pointer to a WindowManagerAddViewParams native structure.
			*/
		public addViewNative(pParams: number): boolean {
			let params = WindowManagerAddViewParams.unmarshal(pParams);

			this.addViewInternal(
				params.HtmlId,
				params.ChildView,
				params.Index != -1 ? params.Index : null
			);

			return true;
		}

		public addViewInternal(parentId: number, childId: number, index?: number): void {
			const parentElement: HTMLElement | SVGElement = this.allActiveElementsById[parentId];
			if (!parentElement) {
				throw `addView: Parent element id ${parentId} not found.`;
			}
			const childElement: HTMLElement | SVGElement = this.allActiveElementsById[childId];
			if (!childElement) {
				throw `addView: Child element id ${parentId} not found.`;
			}

			const alreadyLoaded = this.getIsConnectedToRootElement(childElement);
			const isLoading = !alreadyLoaded && this.getIsConnectedToRootElement(parentElement);

			if (isLoading) {
				this.dispatchEvent(childElement, "loading");
			}

			if (index && index < parentElement.childElementCount) {
				const insertBeforeElement = parentElement.children[index];
				parentElement.insertBefore(childElement, insertBeforeElement);

			} else {
				parentElement.appendChild(childElement);
			}

			if (isLoading) {
				this.dispatchEvent(childElement, "loaded");
			}
		}

		/**
			* Remove a child from a parent element.
			*
			* "Unloading" & "Unloaded" events will be raised if nescessary.
			*/
		public removeView(parentId: number, childId: number): string {
			this.removeViewInternal(parentId, childId);
			return "ok";
		}

		/**
			* Remove a child from a parent element.
			*
			* "Unloading" & "Unloaded" events will be raised if nescessary.
			*/
		public removeViewNative(pParams: number): boolean {
			var params = WindowManagerRemoveViewParams.unmarshal(pParams);
			this.removeViewInternal(params.HtmlId, params.ChildView);
			return true;
		}

		private removeViewInternal(parentId: number, childId: number): void {
			const parentElement: HTMLElement | SVGElement = this.allActiveElementsById[parentId];
			if (!parentElement) {
				throw `removeView: Parent element id ${parentId} not found.`;
			}
			const childElement: HTMLElement | SVGElement = this.allActiveElementsById[childId];
			if (!childElement) {
				throw `removeView: Child element id ${parentId} not found.`;
			}

			const loaded = this.getIsConnectedToRootElement(childElement);

			parentElement.removeChild(childElement);

			if (loaded) {
				this.dispatchEvent(childElement, "unloaded");
			}
		}

		/**
			* Destroy a html element.
			*
			* The element won't be available anymore. Usually indicate the managed
			* version has been scavenged by the GC.
			*/
		public destroyView(viewId: number): string {
			this.destroyViewInternal(viewId);
			return "ok";
		}

		/**
			* Destroy a html element.
			*
			* The element won't be available anymore. Usually indicate the managed
			* version has been scavenged by the GC.
			*/
		public destroyViewNative(pParams: number): boolean {
			let params = WindowManagerDestroyViewParams.unmarshal(pParams);
			this.destroyViewInternal(params.HtmlId);
			return true;
		}

		private destroyViewInternal(viewId: number): void {
			const element: HTMLElement | SVGElement = this.allActiveElementsById[viewId];
			if (!element) {
				throw `destroyView: Element id ${viewId} not found.`;
			}

			if (element.parentElement) {
				element.parentElement.removeChild(element);
				delete this.allActiveElementsById[viewId];
			}
		}

		public getBoundingClientRect(elementId: string): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			var bounds = (<any>htmlElement).getBoundingClientRect();
			return `${bounds.left};${bounds.top};${bounds.right-bounds.left};${bounds.bottom-bounds.top}`;
		}

		public getBBox(elementId: number): string {
			var bbox = this.getBBoxInternal(elementId);

			return `${bbox.x};${bbox.y};${bbox.width};${bbox.height}`;
		}

		public getBBoxNative(pParams: number, pReturn: number): boolean {

			let params = WindowManagerGetBBoxParams.unmarshal(pParams);

			var bbox = this.getBBoxInternal(params.HtmlId);

			var ret = new WindowManagerGetBBoxReturn();
			ret.X = bbox.x;
			ret.Y = bbox.y;
			ret.Width = bbox.width;
			ret.Height = bbox.height;

			ret.marshal(pReturn);

			return true;
		}

		private getBBoxInternal(elementId: number): any {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			return (<any>htmlElement).getBBox();
		}

		/**
			* Use the Html engine to measure the element using specified constraints.
			*
			* @param maxWidth string containing width in pixels. Empty string means infinite.
			* @param maxHeight string containing height in pixels. Empty string means infinite.
			*/
		public measureView(viewId: string, maxWidth: string, maxHeight: string): string {

			var ret = this.measureViewInternal(Number(viewId), maxWidth ? Number(maxWidth) : NaN, maxHeight ? Number(maxHeight) : NaN);

			return `${ret[0]};${ret[1]}`;
		}

		/**
			* Use the Html engine to measure the element using specified constraints.
			*
			* @param maxWidth string containing width in pixels. Empty string means infinite.
			* @param maxHeight string containing height in pixels. Empty string means infinite.
			*/
		public measureViewNative(pParams: number, pReturn: number): boolean {

			var params = WindowManagerMeasureViewParams.unmarshal(pParams);

			var ret = this.measureViewInternal(params.HtmlId, params.AvailableWidth, params.AvailableHeight);

			var ret2 = new WindowManagerMeasureViewReturn();
			ret2.DesiredWidth = ret[0];
			ret2.DesiredHeight = ret[1];

			ret2.marshal(pReturn);

			return true;
		}

		private measureViewInternal(viewId: number, maxWidth: number, maxHeight: number): [number, number] {
			const element = this.allActiveElementsById[viewId] as HTMLElement;
			if (!element) {
				throw `measureView: Element id ${viewId} not found.`;
			}

			const previousWidth = element.style.width;
			const previousHeight = element.style.height;
			const previousPosition = element.style.position;

			try {
				if (!element.isConnected) {
					// If the element is not connected to the DOM, we need it
					// to be connected for the measure to provide a meaningful value.

					let unconnectedRoot = element;
					while (unconnectedRoot.parentElement) {
						// Need to find the top most "unconnected" parent
						// of this element
						unconnectedRoot = unconnectedRoot.parentElement as HTMLElement;
					}

					this.containerElement.appendChild(unconnectedRoot);
				}

				element.style.width = "";
				element.style.height = "";

				// This is required for an unconstrained measure (otherwise the parents size is taken into accound)
				element.style.position = "fixed";

				element.style.maxWidth = Number.isFinite(maxWidth) ? `${maxWidth}px` : "";
				element.style.maxHeight = Number.isFinite(maxHeight) ? `${maxHeight}px` : "";

				if (element.tagName.toUpperCase() === "IMG") {
					const imgElement = element as HTMLImageElement;
					return [imgElement.naturalWidth, imgElement.naturalHeight];
				}
				else {
					const resultWidth = element.offsetWidth ? element.offsetWidth : element.clientWidth;
					const resultHeight = element.offsetHeight ? element.offsetHeight : element.clientHeight;

					/* +0.5 is added to take rounding into account */
					return [resultWidth + 0.5, resultHeight];
				}
			} finally {
				element.style.width = previousWidth;
				element.style.height = previousHeight;
				element.style.position = previousPosition;

				element.style.maxWidth = "";
				element.style.maxHeight = "";
			}
		}

		public setImageRawData(viewId: string, dataPtr: number, width: number, height: number): string {
			const element = this.allActiveElementsById[viewId] as HTMLElement;
			if (!element) {
				throw `setImageRawData: Element id ${viewId} not found.`;
			}

			if (element.tagName.toUpperCase() === "IMG") {
				const imgElement = element as HTMLImageElement;

				var rawCanvas = document.createElement("canvas");
				rawCanvas.width = width;
				rawCanvas.height = height;

				var ctx = rawCanvas.getContext("2d");
				var imgData = ctx.createImageData(width, height);

				var bufferSize = width * height * 4;

				for (var i = 0; i < bufferSize; i += 4) {
					imgData.data[i + 0] = Module.HEAPU8[dataPtr + i + 2];
					imgData.data[i + 1] = Module.HEAPU8[dataPtr + i + 1];
					imgData.data[i + 2] = Module.HEAPU8[dataPtr + i + 0];
					imgData.data[i + 3] = Module.HEAPU8[dataPtr + i + 3];
				}
				ctx.putImageData(imgData, 0, 0);

				imgElement.src = rawCanvas.toDataURL();

				return "ok";
			}
		}


		/**
		 * Sets the provided image with a mono-chrome version of the provided url.
		 * @param viewId the image to manipulate
		 * @param url the source image
		 * @param color the color to apply to the monochrome pixels
		 */
		public setImageAsMonochrome(viewId: string, url: string, color: string): string {
			const element = this.allActiveElementsById[viewId] as HTMLElement;
			if (!element) {
				throw `setImageAsMonochrome: Element id ${viewId} not found.`;
			}

			if (element.tagName.toUpperCase() === "IMG") {

				const imgElement = element as HTMLImageElement;
				var img = new Image();
				img.onload = buildMonochromeImage;
				img.src = url;

				function buildMonochromeImage() {

					// create a colored version of img
					var c = document.createElement('canvas');
					var ctx = c.getContext('2d');

					c.width = img.width;
					c.height = img.height;

					ctx.drawImage(img, 0, 0);
					ctx.globalCompositeOperation = 'source-atop';
					ctx.fillStyle = color;
					ctx.fillRect(0, 0, img.width, img.height);
					ctx.globalCompositeOperation = 'source-over';

					imgElement.src = c.toDataURL();
				}

				return "ok";
			}
			else {
				throw `setImageAsMonochrome: Element id ${viewId} is not an Img.`;
			}
		}

		public setPointerCapture(viewId: string, pointerId: number): string {
			const element = this.allActiveElementsById[viewId] as HTMLElement;
			if (!element) {
				throw `setPointerCapture: Element id ${viewId} not found.`;
			}

			element.setPointerCapture(pointerId);

			return "ok";
		}

		public releasePointerCapture(viewId: string, pointerId: number): string {
			const element = this.allActiveElementsById[viewId] as HTMLElement;
			if (!element) {
				throw `releasePointerCapture: Element id ${viewId} not found.`;
			}

			element.releasePointerCapture(pointerId);

			return "ok";
		}

		public focusView(elementId: string): string {
			const htmlElement: HTMLElement | SVGElement = this.allActiveElementsById[elementId];
			if (!htmlElement) {
				throw `Element id ${elementId} not found.`;
			}

			if (!(htmlElement instanceof HTMLElement)) {
				throw `Element id ${elementId} is not focusable.`;
			}

			htmlElement.focus();

			return "ok";
		}

		/**
			* Set the Html content for an element.
			*
			* Those html elements won't be available as XamlElement in managed code.
			* WARNING: you should avoid mixing this and `addView` for the same element.
			*/
		public setHtmlContent(viewId: number, html: string): string {
			this.setHtmlContentInternal(viewId, html);
			return "ok";
		}

		/**
			* Set the Html content for an element.
			*
			* Those html elements won't be available as XamlElement in managed code.
			* WARNING: you should avoid mixing this and `addView` for the same element.
			*/
		public setHtmlContentNative(pParams: number): boolean {
			let params = WindowManagerSetContentHtmlParams.unmarshal(pParams);

			this.setHtmlContentInternal(params.HtmlId, params.Html);
			return true;
		}

		private setHtmlContentInternal(viewId: number, html: string): void {
			const element: HTMLElement | SVGElement = this.allActiveElementsById[viewId];
			if (!element) {
				throw `setHtmlContent: Element id ${viewId} not found.`;
			}

			element.innerHTML = html;
		}

		/**
			* Remove the loading indicator.
			*
			* In a future version it will also handle the splashscreen.
			*/
		public activate(): string {
			this.removeLoading();
			return "ok";
		}

		private init() {

			if (UnoAppManifest.displayName) {
				document.title = UnoAppManifest.displayName;
			}

		}

		private static initMethods() {
			if (WindowManager.isHosted) {
				console.debug("Hosted Mode: Skipping MonoRuntime initialization ");
			}
			else {
				if (!WindowManager.assembly) {
					WindowManager.assembly = MonoRuntime.assembly_load("Uno.UI");

					if (!WindowManager.assembly) {
						throw `Unable to find assembly Uno.UI`;
					}
				}

				if (!WindowManager.resizeMethod) {
					const type = MonoRuntime.find_class(WindowManager.assembly, "Windows.UI.Xaml", "Window");

					if (!type) {
						throw `Unable to find type Windows.UI.Xaml.Window`;
					}

					WindowManager.resizeMethod = MonoRuntime.find_method(type, "Resize", -1);

					if (!WindowManager.resizeMethod) {
						throw `Unable to find Windows.UI.Xaml.Window.Resize method`;
					}
				}

				if (!WindowManager.dispatchEventMethod) {
					const type = MonoRuntime.find_class(WindowManager.assembly, "Windows.UI.Xaml", "UIElement");
					WindowManager.dispatchEventMethod = MonoRuntime.find_method(type, "DispatchEvent", -1);

					if (!WindowManager.dispatchEventMethod) {
						throw `Unable to find Windows.UI.Xaml.UIElement.DispatchEvent method`;
					}
				}
			}
		}

		private initDom() {
			this.containerElement = (document.getElementById(this.containerElementId) as HTMLDivElement);
			if (!this.containerElement) {
				// If not found, we simply create a new one.
				this.containerElement = document.createElement("div");
				document.body.appendChild(this.containerElement);
			}

			window.addEventListener("resize", x => this.resize());
		}

		private removeLoading() {

			if (!this.loadingElementId) {
				return;
			}

			const element = document.getElementById(this.loadingElementId);
			if (element) {
				element.parentElement.removeChild(element);
			}

			// UWP Window's default background is white.
			const body = document.getElementsByTagName("body")[0];
			body.style.backgroundColor = '#fff';
		}

		private resize() {

			if (WindowManager.isHosted) {
				UnoDispatch.resize(`${window.innerWidth};${window.innerHeight}`);
			}
			else {
				const sizeStr = this.getMonoString(`${window.innerWidth};${window.innerHeight}`);
				MonoRuntime.call_method(WindowManager.resizeMethod, null, [sizeStr]);
			}
		}

		private dispatchEvent(element: HTMLElement | SVGElement, eventName: string, eventPayload: string = null): boolean {
			const htmlId = element.getAttribute("XamlHandle");
			
			// console.debug(`${element.getAttribute("id")}: Raising event ${eventName}.`);

			if (!htmlId) {
				throw `No attribute XamlHandle on element ${element}. Can't raise event.`;
			}

			if (WindowManager.isHosted) {
				// Dispatch to the C# backed UnoDispatch class. Events propagated
				// this way always succeed because synchronous calls are not possible
				// between the host and the browser, unlike wasm.
				UnoDispatch.dispatch(htmlId, eventName, eventPayload);
				return true;
			}
			else {
				const htmlIdStr = this.getMonoString(htmlId);
				const eventNameStr = this.getMonoString(eventName);
				const eventPayloadStr = this.getMonoString(eventPayload);

				var handledHandle = MonoRuntime.call_method(WindowManager.dispatchEventMethod, null, [htmlIdStr, eventNameStr, eventPayloadStr]);
				var handledStr = this.fromMonoString(handledHandle);
				var handled = handledStr == "True";
				return handled;
			}
		}

		private getMonoString(str: string): Interop.IMonoStringHandle {
			return str ? MonoRuntime.mono_string(str) : null;
		}

		private fromMonoString(strHandle: Interop.IMonoStringHandle): string {
			return strHandle ? MonoRuntime.conv_string(strHandle) : "";
		}

		private getIsConnectedToRootElement(element: HTMLElement | SVGElement): boolean {
			const rootElement = this.rootContent;

			if (!rootElement) {
				return false;
			}
			return rootElement === element || rootElement.contains(element);
		}
	}

	if (typeof define === 'function') {
		define(
			['AppManifest'],
			() => {
				if (document.readyState === "loading") {
					document.addEventListener("DOMContentLoaded", () => WindowManager.setupSplashScreen());
				} else {
					WindowManager.setupSplashScreen();
				}
			}
		);
	}
	else {
		throw `The Uno.Wasm.Boostrap is not up to date, please upgrade to a later version`;
	}
}
